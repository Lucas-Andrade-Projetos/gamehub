using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.Interfaces;

public interface IBatalhaRuralGameService
{
    Task<BatalhaRuralGame> CreateGameAsync(string userId1, string nickname1, string userId2, string nickname2);

    Task<BatalhaRuralGame?> GetGameAsync(string gameId);

    Task<PlacementResult?> PlaceTokenAsync(BatalhaRuralPlayer player, int tokenIndex, int positionX, int positionY, Direction direction);

    Task<bool> SetPlayerReadyAsync(string gameId, string userId);
}
