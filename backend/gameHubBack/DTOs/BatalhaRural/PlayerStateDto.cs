using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class PlayerStateDto
{
    public required string Id { get; set; }
    public required string Nickname { get; set; }
    public required PlayerNum PlayerNum { get; set; }
    public required PlayerStatus PlayerStatus { get; set; }
    public required string[][] Board { get; set; }
    public required List<TokenDto> Tokens { get; set; }
}
