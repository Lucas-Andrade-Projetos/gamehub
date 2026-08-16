using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class PlaceTokenResultDto
{
    public required bool Success { get; set; }
    public PlacementFailureReason? Reason { get; set; }
    public required PlayerStateDto Player { get; set; }
}
