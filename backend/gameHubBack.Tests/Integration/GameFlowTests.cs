using System.Net;
using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using gameHubBack.Services.Rooms;
using Microsoft.Extensions.DependencyInjection;
using static gameHubBack.Tests.Integration.PlayerClient;

namespace gameHubBack.Tests.Integration;

// End-to-end matches over the real hub and HTTP API, with two simulated players.
public class GameFlowTests
{
    private static readonly BatalhaRuralTimings Normal = new();

    private sealed record Match(GameHubFactory Factory, PlayerClient P1, PlayerClient P2, string Code, string GameId) : IAsyncDisposable
    {
        public PlayerClient Other(PlayerClient player) => player == P1 ? P2 : P1;

        public async ValueTask DisposeAsync()
        {
            await P1.DisposeAsync();
            await P2.DisposeAsync();
            await Factory.DisposeAsync();
        }
    }

    // lobby -> both ready -> game created
    private static async Task<Match> CreateMatchAsync(BatalhaRuralTimings timings)
    {
        var factory = new GameHubFactory(timings);
        var p1 = await factory.ConnectPlayerAsync("Alice");
        var p2 = await factory.ConnectPlayerAsync("Bob");

        var code = await p1.Hub.InvokeAsync<string>("CreateRoom");
        await p2.Hub.InvokeAsync("JoinRoom", code);
        await p1.Hub.InvokeAsync("SetReady", code, true);
        await p2.Hub.InvokeAsync("SetReady", code, true);

        var gameId = await NextAsync(p1.GameReady);
        Assert.Equal(gameId, await NextAsync(p2.GameReady));

        return new Match(factory, p1, p2, code, gameId);
    }

    private static async Task ConfirmPlacementAsync(Match match, PlayerClient player)
    {
        var result = await player.PlaceAsync(match.GameId, TestData.StandardLayout());
        Assert.True(result.Success, $"placement failed: {result.Reason}");
        await player.Hub.InvokeAsync("NotifyPlacementReady", match.GameId, match.Code);
    }

    private static async Task<Match> StartBattleAsync(BatalhaRuralTimings timings)
    {
        var match = await CreateMatchAsync(timings);

        await ConfirmPlacementAsync(match, match.P1);
        await ConfirmPlacementAsync(match, match.P2);

        await NextAsync(match.P1.BattleStarting);
        await NextAsync(match.P2.BattleStarting);

        return match;
    }

    private static async Task<PlayerClient> CurrentTurnPlayerAsync(Match match)
    {
        var state = await match.P1.GetGameAsync(match.GameId);
        return state.CurrentTurnPlayerNum == state.ViewerPlayerNum ? match.P1 : match.P2;
    }

    [Fact]
    public async Task Full_match_from_lobby_to_victory_then_rematch()
    {
        await using var match = await StartBattleAsync(Normal);

        var state = await match.P1.GetGameAsync(match.GameId);
        Assert.Equal(GameStatus.InProgress, state.Status);
        // the opponent's board is never sent, only one's own
        var opponentView = state.Players.Single(p => p.PlayerNum != state.ViewerPlayerNum);
        Assert.All(opponentView.Board.SelectMany(r => r), c => Assert.Equal(BoardFactory.EmptyTile, c));
        Assert.Empty(opponentView.Tokens);

        // whoever starts sinks everything: hits keep the turn, so this is one uninterrupted run
        var attacker = await CurrentTurnPlayerAsync(match);
        var defender = match.Other(attacker);
        var defenderState = await defender.GetGameAsync(match.GameId);
        var defenderBoard = defenderState.Players.Single(p => p.PlayerNum == defenderState.ViewerPlayerNum).Board;

        foreach (var (x, y) in TestData.OccupiedCells(defenderBoard))
            await attacker.Hub.InvokeAsync("Attack", match.GameId, match.Code, x, y);

        var ended = await NextAsync(match.P1.GameEnded);
        Assert.Equal("Normal", ended.Reason);
        Assert.Equal(ended.WinnerPlayerNum, (await NextAsync(match.P2.GameEnded)).WinnerPlayerNum);

        var final = await attacker.GetGameAsync(match.GameId);
        Assert.Equal(GameStatus.Ended, final.Status);
        Assert.Equal(final.ViewerPlayerNum, ended.WinnerPlayerNum);
        Assert.Equal(PlayerStatus.Winner, final.Players.Single(p => p.PlayerNum == final.ViewerPlayerNum).PlayerStatus);

        // rematch: first vote is broadcast, second one starts a brand-new game in placement
        await match.P1.Hub.InvokeAsync("RequestRematch", match.Code);
        await NextAsync(match.P2.RoomUpdated, r => r.Player1?.WantsRematch == true || r.Player2?.WantsRematch == true);
        await match.P2.Hub.InvokeAsync("RequestRematch", match.Code);

        var rematchId = await NextAsync(match.P1.GameReady);
        Assert.NotEqual(match.GameId, rematchId);
        Assert.Equal(rematchId, await NextAsync(match.P2.GameReady));
        Assert.Equal(GameStatus.Preparing, (await match.P2.GetGameAsync(rematchId)).Status);
    }

    [Fact]
    public async Task Rematch_is_not_available_while_the_game_is_running()
    {
        await using var match = await StartBattleAsync(Normal);

        var error = await Assert.ThrowsAsync<HubException>(() => match.P1.Hub.InvokeAsync("RequestRematch", match.Code));
        Assert.Contains("não terminou", error.Message);
    }

    [Fact]
    public async Task Placement_timeout_places_pieces_for_whoever_did_not_confirm_and_starts_the_battle()
    {
        await using var match = await CreateMatchAsync(new BatalhaRuralTimings { PlacementDuration = TimeSpan.FromSeconds(1) });

        await ConfirmPlacementAsync(match, match.P1);

        await NextAsync(match.P1.BattleStarting);
        await NextAsync(match.P2.BattleStarting);
        await NextAsync(match.P2.Messages, m => m.Text.Contains("aleatoriamente") && m.Text.Contains("Bob"));

        var game = await match.Factory.GetGameFromDbAsync(match.GameId);
        Assert.Equal(GameStatus.InProgress, game.Status);
        var bob = game.Players.Single(p => p.UserId == match.P2.UserId);
        Assert.Equal(TestData.TotalPieceCells, BoardFactory.CountOccupied(bob.BoardTiles));
        // Alice's confirmed layout is untouched: bull still on row 4
        var alice = game.Players.Single(p => p.UserId == match.P1.UserId);
        Assert.Equal("B", alice.BoardTiles[3][0]);
    }

    [Fact]
    public async Task Placement_timer_does_not_fire_once_both_confirmed()
    {
        await using var match = await StartBattleAsync(new BatalhaRuralTimings { PlacementDuration = TimeSpan.FromSeconds(1) });

        await AssertNoEventAsync(match.P1.BattleStarting, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Nobody_playing_ends_the_game_by_inactivity()
    {
        await using var match = await StartBattleAsync(new BatalhaRuralTimings { TurnDuration = TimeSpan.FromMilliseconds(300) });

        var ended = await NextAsync(match.P1.GameEnded);

        Assert.Equal("Inactivity", ended.Reason);
        var game = await match.Factory.GetGameFromDbAsync(match.GameId);
        Assert.Equal(GameStatus.Ended, game.Status);
        var loser = game.Players.Single(p => p.PlayerStatus == PlayerStatus.Loser);
        Assert.Equal(3, loser.ConsecutiveTimeouts);
        Assert.Equal(ended.WinnerPlayerNum, game.Players.Single(p => p.PlayerStatus == PlayerStatus.Winner).PlayerNum);
    }

    [Fact]
    public async Task Leaving_mid_match_is_an_immediate_forfeit()
    {
        await using var match = await StartBattleAsync(Normal);

        await match.P1.Hub.InvokeAsync("LeaveRoom", match.Code);

        var ended = await NextAsync(match.P2.GameEnded);
        Assert.Equal("Abandonment", ended.Reason);
        var state = await match.P2.GetGameAsync(match.GameId);
        Assert.Equal(state.ViewerPlayerNum, ended.WinnerPlayerNum);
    }

    [Fact]
    public async Task Dropping_without_reconnecting_forfeits_after_the_grace_period()
    {
        await using var match = await StartBattleAsync(new BatalhaRuralTimings { ReconnectGrace = TimeSpan.FromMilliseconds(500) });

        await match.P1.Hub.StopAsync();

        var ended = await NextAsync(match.P2.GameEnded);
        Assert.Equal("Abandonment", ended.Reason);
        Assert.Equal(GameStatus.Ended, (await match.P2.GetGameAsync(match.GameId)).Status);
    }

    [Fact]
    public async Task Reconnecting_in_time_cancels_the_forfeit()
    {
        await using var match = await StartBattleAsync(new BatalhaRuralTimings { ReconnectGrace = TimeSpan.FromSeconds(1) });

        await match.P1.Hub.StopAsync();
        await NextAsync(match.P2.Messages, m => m.Text.Contains("caiu"));

        await match.P1.ConnectAsync();
        var room = await match.P1.Hub.InvokeAsync<gameHubBack.DTOs.Rooms.RoomStateDto?>("Rejoin", match.Code);
        Assert.NotNull(room);
        Assert.Equal(match.GameId, room!.GameId);

        await AssertNoEventAsync(match.P2.GameEnded, TimeSpan.FromSeconds(2));
        Assert.Equal(GameStatus.InProgress, (await match.P1.GetGameAsync(match.GameId)).Status);

        // and the reconnected player can keep playing
        var attacker = await CurrentTurnPlayerAsync(match);
        await attacker.Hub.InvokeAsync("Attack", match.GameId, match.Code, 9, 9);
    }

    [Fact]
    public async Task Room_is_cleaned_up_after_everyone_leaves_a_finished_match()
    {
        var timings = new BatalhaRuralTimings
        {
            ReconnectGrace = TimeSpan.FromMilliseconds(300),
            RoomCleanupDelay = TimeSpan.FromMilliseconds(500)
        };
        await using var match = await StartBattleAsync(timings);
        await match.P1.Hub.InvokeAsync("LeaveRoom", match.Code);
        await NextAsync(match.P2.GameEnded);

        await match.P2.Hub.StopAsync();
        await Task.Delay(1500);

        var registry = match.Factory.Services.GetRequiredService<IRoomRegistry>();
        Assert.Null(registry.GetRoom(match.Code));
    }
}

// The hub and the API must not trust anything the client claims.
public class GameSecurityTests
{
    private static async Task<(GameHubFactory Factory, PlayerClient P1, PlayerClient P2, string Code, string GameId)> InPlacementAsync()
    {
        var factory = new GameHubFactory();
        var p1 = await factory.ConnectPlayerAsync("Alice");
        var p2 = await factory.ConnectPlayerAsync("Bob");

        var code = await p1.Hub.InvokeAsync<string>("CreateRoom");
        await p2.Hub.InvokeAsync("JoinRoom", code);
        await p1.Hub.InvokeAsync("SetReady", code, true);
        await p2.Hub.InvokeAsync("SetReady", code, true);

        return (factory, p1, p2, code, await NextAsync(p1.GameReady));
    }

    [Fact]
    public async Task Outsiders_cannot_act_on_someone_elses_room()
    {
        var (factory, p1, p2, code, gameId) = await InPlacementAsync();
        await using var _ = factory;
        await using var outsider = await factory.ConnectPlayerAsync("Mallory");

        await Assert.ThrowsAsync<HubException>(() => outsider.Hub.InvokeAsync("SendMessage", code, "spam"));
        await Assert.ThrowsAsync<HubException>(() => outsider.Hub.InvokeAsync("ReturnToRoom", code));
        await Assert.ThrowsAsync<HubException>(() => outsider.Hub.InvokeAsync("SetReady", code, true));
        await Assert.ThrowsAsync<HubException>(() => outsider.Hub.InvokeAsync("Attack", gameId, code, 0, 0));
        await Assert.ThrowsAsync<HubException>(() => outsider.Hub.InvokeAsync("NotifyPlacementReady", gameId, code));
        await Assert.ThrowsAsync<HubException>(() => outsider.Hub.InvokeAsync("RequestRematch", code));

        Assert.Equal(HttpStatusCode.Forbidden, (await outsider.Http.GetAsync($"api/batalha-rural/{gameId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outsider.Http.PutAsJsonAsyncRaw($"api/batalha-rural/{gameId}/placement")).StatusCode);

        await p1.DisposeAsync();
        await p2.DisposeAsync();
    }

    [Fact]
    public async Task Confirming_without_placing_is_rejected()
    {
        var (factory, p1, p2, code, gameId) = await InPlacementAsync();
        await using var _ = factory;

        var error = await Assert.ThrowsAsync<HubException>(() => p1.Hub.InvokeAsync("NotifyPlacementReady", gameId, code));
        Assert.Contains("Posicione", error.Message);

        await p1.DisposeAsync();
        await p2.DisposeAsync();
    }

    [Fact]
    public async Task Running_match_cannot_be_reset_restarted_or_rearranged()
    {
        var (factory, p1, p2, code, gameId) = await InPlacementAsync();
        await using var _ = factory;
        foreach (var p in new[] { p1, p2 })
        {
            Assert.True((await p.PlaceAsync(gameId, TestData.StandardLayout())).Success);
            await p.Hub.InvokeAsync("NotifyPlacementReady", gameId, code);
        }
        await NextAsync(p1.BattleStarting);

        await Assert.ThrowsAsync<HubException>(() => p1.Hub.InvokeAsync("ReturnToRoom", code));
        await Assert.ThrowsAsync<HubException>(() => p1.Hub.InvokeAsync("SetReady", code, true));

        var moved = await p1.PlaceAsync(gameId, TestData.VerticalLayout());
        Assert.False(moved.Success);
        Assert.Equal(PlacementFailureReason.NotAllowed, moved.Reason);

        await Assert.ThrowsAsync<HubException>(() => p1.Hub.InvokeAsync("Attack", "some-other-game", code, 0, 0));

        var game = await factory.GetGameFromDbAsync(gameId);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.All(game.Players, pl => Assert.Equal("B", pl.BoardTiles[3][0]));

        await p1.DisposeAsync();
        await p2.DisposeAsync();
    }

    [Fact]
    public async Task Chat_messages_are_trimmed_and_capped()
    {
        var (factory, p1, p2, code, _) = await InPlacementAsync();
        await using var __ = factory;

        await p1.Hub.InvokeAsync("SendMessage", code, "   ");
        await p1.Hub.InvokeAsync("SendMessage", code, "  oi  " + new string('a', 1000));

        var message = await NextAsync(p2.Messages, m => !m.IsSystem);
        Assert.StartsWith("oi", message.Text);
        Assert.Equal(300, message.Text.Length);

        await p1.DisposeAsync();
        await p2.DisposeAsync();
    }
}

internal static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PutAsJsonAsyncRaw(this HttpClient client, string url) =>
        client.PutAsync(url, new StringContent("""{"tokens":[]}""", System.Text.Encoding.UTF8, "application/json"));
}
