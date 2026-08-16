using gameHubBack.DTOs.BatalhaRural;
using gameHubBack.Enums.BatalhaRural;
using gameHubBack.Extensions;
using gameHubBack.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace gameHubBack.Controllers;

[ApiController]
[Route("api/batalha-rural")]
public class BatalhaRuralController(IBatalhaRuralGameService gameService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GameStateDto>> CreateGame(CreateGameDto createGameDto)
    {
        var game = await gameService.CreateGameAsync(createGameDto.Player1Nickname, createGameDto.Player2Nickname);

        return game.ToDto();
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<GameStateDto>> GetGame(string gameId)
    {
        var game = await gameService.GetGameAsync(gameId);

        if (game == null) return NotFound();

        return game.ToDto();
    }

    [HttpPost("{gameId}/players/{playerNum}/tokens/{tokenIndex:int}/place")]
    public async Task<ActionResult<PlaceTokenResultDto>> PlaceToken(string gameId, PlayerNum playerNum, int tokenIndex, PlaceTokenDto placeTokenDto)
    {
        var game = await gameService.GetGameAsync(gameId);

        if (game == null) return NotFound("Game not found");

        var player = game.Players.SingleOrDefault(p => p.PlayerNum == playerNum);

        if (player == null) return NotFound("Player not found");

        var result = await gameService.PlaceTokenAsync(player, tokenIndex, placeTokenDto.PositionX, placeTokenDto.PositionY, placeTokenDto.Direction);

        if (result == null) return NotFound("Token not found");

        return new PlaceTokenResultDto
        {
            Success = result.Success,
            Reason = result.Reason,
            Player = player.ToDto()
        };
    }
}
