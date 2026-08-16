using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.Interfaces;

public interface IBatalhaRuralGameService
{
    Task<BatalhaRuralGame> CreateGameAsync(string player1Nickname, string player2Nickname);

    Task<BatalhaRuralGame?> GetGameAsync(string gameId);

    Task<PlacementResult?> PlaceTokenAsync(BatalhaRuralPlayer player, int tokenIndex, int positionX, int positionY, Direction direction);
}
