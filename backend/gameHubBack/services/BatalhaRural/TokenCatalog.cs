using gameHubBack.Entities.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.Services.BatalhaRural;

public static class TokenCatalog
{
    private static readonly Dictionary<TokenType, (string Name, string Character, int Size)> Metadata = new()
    {
        [TokenType.Chicken] = ("Chicken", "c", 1),
        [TokenType.Bull] = ("Bull", "B", 5),
        [TokenType.Cow] = ("Cow", "C", 2),
        [TokenType.Horse] = ("Horse", "H", 2),
    };

    public static string GetName(TokenType type) => Metadata[type].Name;

    public static string GetCharacter(TokenType type) => Metadata[type].Character;

    public static int GetSize(TokenType type) => Metadata[type].Size;

    public static List<GameToken> CreateDefaultTokens() =>
    [
        new() { Type = TokenType.Chicken, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Chicken, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Chicken, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Bull, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Cow, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Horse, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Horse, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
        new() { Type = TokenType.Cow, PositionX = 0, PositionY = 0, Direction = Direction.Centered },
    ];
}