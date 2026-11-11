using System.Security.Claims;
using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.Extensions;
using gameHubBack.Interfaces;
using gameHubBack.Services.BatalhaRural;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gameHubBack.Controllers;

[Authorize]
[ApiController]
[Route("api/batalha-rural")]
public class BatalhaRuralController(IBatalhaRuralGameService gameService) : ControllerBase
{
    [HttpGet("{gameId}")]
    public async Task<ActionResult<GameStateDto>> GetGame(string gameId)
    {
        var game = await gameService.GetGameAsync(gameId);

        if (game == null) return NotFound();
        if (game.Players.All(p => p.UserId != CurrentUserId)) return Forbid();

        return game.ToDto(CurrentUserId);
    }

    [HttpPut("{gameId}/placement")]
    public async Task<ActionResult<PlacementResultDto>> PlaceTokens(string gameId, PlaceTokensDto placeTokensDto)
    {
        var placements = placeTokensDto.Tokens
            .Select(t => new TokenPlacement(t.Index, t.PositionX, t.PositionY, t.Direction))
            .ToList();

        var result = await gameService.PlaceAllTokensAsync(gameId, CurrentUserId, placements);

        if (result == null) return NotFound("Game not found");

        return new PlacementResultDto
        {
            Success = result.Success,
            Reason = result.Reason,
            TokenIndex = result.TokenIndex
        };
    }

    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User not authenticated");
}
