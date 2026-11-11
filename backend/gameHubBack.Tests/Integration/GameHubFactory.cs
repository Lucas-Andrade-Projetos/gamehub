using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using gameHubBack.Data;
using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.DTOs.Rooms;
using gameHubBack.Entities;
using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Interfaces;
using gameHubBack.Services.BatalhaRural;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace gameHubBack.Tests.Integration;

// The real app (controllers, hub, auth, timers) on an in-memory database, with configurable
// clocks so timeouts can be tested in milliseconds.
public sealed class GameHubFactory(BatalhaRuralTimings? timings = null) : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public BatalhaRuralTimings Timings { get; } = timings ?? new BatalhaRuralTimings();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["TokenKey"] = "integration-tests-signing-key-that-is-definitely-longer-than-sixty-four-characters!!",
            ["ConnectionStrings:DefaultConnection"] = "Host=unused"
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<BatalhaRuralTimings>();
            services.AddSingleton(Timings);
        });
    }

    public async Task<PlayerClient> ConnectPlayerAsync(string nickname)
    {
        var user = new AppUser
        {
            Email = $"{nickname.ToLowerInvariant()}@test.local",
            Nickname = nickname,
            NormalizedEmail = $"{nickname.ToUpperInvariant()}@TEST.LOCAL",
            NormalizedNickname = nickname.ToUpperInvariant(),
            PasswordHash = [],
            PasswordSalt = []
        };

        string token;
        using (var scope = Services.CreateScope())
        {
            token = scope.ServiceProvider.GetRequiredService<ITokenService>().CreateToken(user);
        }

        var client = new PlayerClient(this, user.Id, nickname, token);
        await client.ConnectAsync();
        return client;
    }

    public async Task<BatalhaRuralGame> GetGameFromDbAsync(string gameId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.BatalhaRuralGames.AsNoTracking().Include(g => g.Players).SingleAsync(g => g.Id == gameId);
    }
}

public sealed class PlayerClient : IAsyncDisposable
{
    public static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(10);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly GameHubFactory _factory;
    private readonly string _token;

    public string UserId { get; }
    public string Nickname { get; }
    public HubConnection Hub { get; private set; } = null!;
    public HttpClient Http { get; }

    public Channel<string> GameReady { get; private set; } = null!;
    public Channel<bool> BattleStarting { get; private set; } = null!;
    public Channel<GameEndedDto> GameEnded { get; private set; } = null!;
    public Channel<RoomStateDto> RoomUpdated { get; private set; } = null!;
    public Channel<ChatMessageDto> Messages { get; private set; } = null!;
    public Channel<bool> TurnTimedOut { get; private set; } = null!;

    public PlayerClient(GameHubFactory factory, string userId, string nickname, string token)
    {
        _factory = factory;
        _token = token;
        UserId = userId;
        Nickname = nickname;

        Http = factory.CreateClient();
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // (Re)connects with a fresh connection - also used to simulate a reload after a drop.
    public async Task ConnectAsync()
    {
        GameReady = Channel.CreateUnbounded<string>();
        BattleStarting = Channel.CreateUnbounded<bool>();
        GameEnded = Channel.CreateUnbounded<GameEndedDto>();
        RoomUpdated = Channel.CreateUnbounded<RoomStateDto>();
        Messages = Channel.CreateUnbounded<ChatMessageDto>();
        TurnTimedOut = Channel.CreateUnbounded<bool>();

        var server = _factory.Server;

        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(server.BaseAddress, "hubs/game"), options =>
            {
                options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(_token);
                options.Transports = HttpTransportType.LongPolling;
            })
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();

        Hub.On<string>("GameReady", id => GameReady.Writer.TryWrite(id));
        Hub.On("BattleStarting", () => BattleStarting.Writer.TryWrite(true));
        Hub.On<GameEndedDto>("GameEnded", e => GameEnded.Writer.TryWrite(e));
        Hub.On<RoomStateDto>("RoomUpdated", r => RoomUpdated.Writer.TryWrite(r));
        Hub.On<ChatMessageDto>("ChatMessageReceived", m => Messages.Writer.TryWrite(m));
        Hub.On<object>("TurnTimedOut", _ => TurnTimedOut.Writer.TryWrite(true));

        await Hub.StartAsync();
    }

    public async Task<GameStateDto> GetGameAsync(string gameId)
    {
        var response = await Http.GetAsync($"api/batalha-rural/{gameId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameStateDto>(Json))!;
    }

    public async Task<PlacementResultDto> PlaceAsync(string gameId, IEnumerable<TokenPlacement> layout)
    {
        var body = new
        {
            tokens = layout.Select(t => new { index = t.Index, positionX = t.PositionX, positionY = t.PositionY, direction = t.Direction.ToString() })
        };

        var response = await Http.PutAsJsonAsync($"api/batalha-rural/{gameId}/placement", body, Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlacementResultDto>(Json))!;
    }

    public static async Task<T> NextAsync<T>(Channel<T> channel, Func<T, bool>? match = null, TimeSpan? timeout = null)
    {
        using var cts = new CancellationTokenSource(timeout ?? DefaultWait);

        try
        {
            while (true)
            {
                var item = await channel.Reader.ReadAsync(cts.Token);
                if (match == null || match(item)) return item;
            }
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"No matching {typeof(T).Name} event within {(timeout ?? DefaultWait).TotalSeconds}s");
        }
    }

    public static async Task AssertNoEventAsync<T>(Channel<T> channel, TimeSpan window)
    {
        using var cts = new CancellationTokenSource(window);

        try
        {
            var item = await channel.Reader.ReadAsync(cts.Token);
            Assert.Fail($"Unexpected {typeof(T).Name} event: {JsonSerializer.Serialize(item)}");
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Hub.DisposeAsync();
        Http.Dispose();
    }
}
