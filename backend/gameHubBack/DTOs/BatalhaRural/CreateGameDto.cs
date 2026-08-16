using System.ComponentModel.DataAnnotations;

namespace gameHubBack.DTOs.BatalhaRural;

public class CreateGameDto
{
    [Required]
    public required string Player1Nickname { get; set; }

    [Required]
    public required string Player2Nickname { get; set; }
}
