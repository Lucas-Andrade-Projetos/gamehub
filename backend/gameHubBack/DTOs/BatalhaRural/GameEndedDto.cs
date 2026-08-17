using gameHubBack.Enums.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class GameEndedDto
{
    public required PlayerNum WinnerPlayerNum { get; set; }
    public required string Reason { get; set; }
}
