using gameHubBack.Services.BatalhaRural;

namespace gameHubBack.DTOs.BatalhaRural;

public class PlacementResultDto
{
    public required bool Success { get; set; }
    public PlacementFailureReason? Reason { get; set; }
    public int? TokenIndex { get; set; }
}
