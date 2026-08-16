using System.Security.Claims;
using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.Extensions;
using gameHubBack.Interfaces;
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

    [HttpPost("{gameId}/tokens/{tokenIndex:int}/place")]
    public async Task<ActionResult<PlaceTokenResultDto>> PlaceToken(string gameId, int tokenIndex, PlaceTokenDto placeTokenDto)
    {
        var game = await gameService.GetGameAsync(gameId);

        if (game == null) return NotFound("Game not found");

        var player = game.Players.SingleOrDefault(p => p.UserId == CurrentUserId);

        if (player == null) return Forbid();

        var result = await gameService.PlaceTokenAsync(player, tokenIndex, placeTokenDto.PositionX, placeTokenDto.PositionY, placeTokenDto.Direction);

        if (result == null) return NotFound("Token not found");

        return new PlaceTokenResultDto
        {
            Success = result.Success,
            Reason = result.Reason,
            Player = player.ToDto(CurrentUserId)
        };
    }

    private string CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User not authenticated");
}
