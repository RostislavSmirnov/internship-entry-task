using Microsoft.AspNetCore.Mvc;
using X0Game.DTOs;
using X0Game.Interfaices;

namespace X0Game.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IGameSerice _gameService;

    public GameController(ILogger<GameController> logger, IGameSerice gameSerice)
    {
        _logger = logger;
        _gameService = gameSerice;
    }


    [HttpPost]
    public async Task<IActionResult> StartGame([FromBody] GameStartModelDTO gameStartParameters)
    {
        _logger.LogInformation("Поступление запроса на начало игры с параметрами: {@GameStartParameters}", gameStartParameters);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Некорректные параметры для начала игры: {@ModelState}", ModelState);
            return BadRequest("Указаны некоректные параметры для начала игры");
        }
        if (gameStartParameters.NextPlayer != "x" && gameStartParameters.NextPlayer != "o")
        {
            _logger.LogWarning("Некорректный выбор игрока для начала игры: {@NextPlayer}", gameStartParameters.NextPlayer); 
            return BadRequest("Уазаны некорректные параметры начального хода для старта игры");
        }
        try
        {
            GameShowDTO gameToShow = await _gameService.StartNewGameAsync(gameStartParameters);
            Response.Headers.ETag = new Microsoft.Extensions.Primitives.StringValues($"\"{gameToShow.Version}\"");
            return CreatedAtAction(nameof(GetGame), new { gameId = gameToShow.GameId}, gameToShow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при старте новой игры с параметрами: {@GameStartParameters}", gameStartParameters);
            throw;
        }
    }


    [HttpGet]
    public async Task<ActionResult<GameShowDTO>> GetGame(int gameId)
    {
        _logger.LogInformation("Поступление запроса на получение игры с ID: {GameId}", gameId);
        if (gameId <= 0)
        {
            _logger.LogWarning("Некорректный ID для поиска игры: {GameId}", gameId);
            return BadRequest("Некоректный ID для поиска");
        }
        try
        {
            GameShowDTO gameToNeedShow = await _gameService.GetGameAsync(gameId);
            if (gameToNeedShow == null)
            {
                _logger.LogWarning("Игра с ID {GameId} не найдена", gameId);
                return NotFound("Игра с указанным ID не найдена");
            }
            return Ok(gameToNeedShow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении игры с ID {GameId}", gameId);
            throw;
        }
    }


    [HttpPost("{gameId}/moves")]
    public async Task<ActionResult<GameShowDTO>> MakeMove(int gameId, [FromBody] GameMoveDTO MoveParameters)
    {
        _logger.LogInformation("Поступление запроса на ход в игре с ID: {GameId}, координаты: ({X}, {Y})", gameId, MoveParameters.X, MoveParameters.Y);
        if (gameId <= 0 || MoveParameters.X < 0 || MoveParameters.Y < 0)
        {
            _logger.LogWarning("Некорректные параметры для хода в игре: {GameId}, координаты: ({X}, {Y})", gameId, MoveParameters.X, MoveParameters.Y);
            return BadRequest("Некорректные параметры для хода в игре");
        }
        
        GameShowDTO gameIsReal = await _gameService.GetGameAsync(gameId);
        
        if (gameIsReal == null)
        {
            _logger.LogWarning("Игра с ID {GameId} не найдена", gameId);
            return NotFound("Игра с указанным ID не найдена");
        }

        try
        {
            GameShowDTO result = await _gameService.MakeMoveAsync(gameId, MoveParameters);
            Response.Headers.ETag = new Microsoft.Extensions.Primitives.StringValues($"\"{result.Version}\"");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ошибка при попытке сделать ход");
            throw;
        }
    }
}
