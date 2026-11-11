using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.Interfaces;

public interface IBatalhaRuralGameService
{
    Task<BatalhaRuralGame> CreateGameAsync(string userId1, string nickname1, string userId2, string nickname2);

    Task<BatalhaRuralGame?> GetGameAsync(string gameId);

    Task<PlacementResult?> PlaceAllTokensAsync(string gameId, string userId, IReadOnlyList<TokenPlacement> placements);

    Task<ReadyResult> SetPlayerReadyAsync(string gameId, string userId);

    Task<List<string>?> AutoCompletePlacementAsync(string gameId);

    Task<AttackResult?> AttackAsync(string gameId, string attackerUserId, int x, int y);

    Task<AttackResult?> TimeoutTurnAsync(string gameId);

    Task<DateTime?> RefreshTurnDeadlineAsync(string gameId, string rejoiningUserId);

    Task<int> CancelUnfinishedGamesAsync();

    Task<AttackResult?> ForfeitAsync(string gameId, string disconnectedUserId);
}
