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
        _logger.LogInformation("����������� ������� �� ������ ���� � �����������: {@GameStartParameters}", gameStartParameters);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("������������ ��������� ��� ������ ����: {@ModelState}", ModelState);
            return BadRequest("������� ����������� ��������� ��� ������ ����");
        }
        if (gameStartParameters.NextPlayer != "x" && gameStartParameters.NextPlayer != "o")
        {
            _logger.LogWarning("������������ ����� ������ ��� ������ ����: {@NextPlayer}", gameStartParameters.NextPlayer); 
            return BadRequest("������ ������������ ��������� ���������� ���� ��� ������ ����");
        }
        try
        {
            GameShowDTO gameToShow = await _gameService.StartNewGameAsync(gameStartParameters);
            Response.Headers.ETag = new Microsoft.Extensions.Primitives.StringValues($"\"{gameToShow.Version}\"");
            return CreatedAtAction(nameof(GetGame), new { gameId = gameToShow.GameId}, gameToShow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "������ ��� ������ ����� ���� � �����������: {@GameStartParameters}", gameStartParameters);
            throw;
        }
    }


    [HttpGet]
    public async Task<ActionResult<GameShowDTO>> GetGame(int gameId)
    {
        _logger.LogInformation("����������� ������� �� ��������� ���� � ID: {GameId}", gameId);
        if (gameId <= 0)
        {
            _logger.LogWarning("������������ ID ��� ������ ����: {GameId}", gameId);
            return BadRequest("����������� ID ��� ������");
        }
        try
        {
            GameShowDTO gameToNeedShow = await _gameService.GetGameAsync(gameId);
            if (gameToNeedShow == null)
            {
                _logger.LogWarning("���� � ID {GameId} �� �������", gameId);
                return NotFound("���� � ��������� ID �� �������");
            }
            return Ok(gameToNeedShow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "������ ��� ��������� ���� � ID {GameId}", gameId);
            throw;
        }
    }


    [HttpPost("{gameId}/moves")]
    public async Task<ActionResult<GameShowDTO>> MakeMove(int gameId, [FromBody] GameMoveDTO MoveParameters)
    {
        _logger.LogInformation("����������� ������� �� ��� � ���� � ID: {GameId}, ����������: ({X}, {Y})", gameId, MoveParameters.X, MoveParameters.Y);
        if (gameId <= 0 || MoveParameters.X < 0 || MoveParameters.Y < 0)
        {
            _logger.LogWarning("������������ ��������� ��� ���� � ����: {GameId}, ����������: ({X}, {Y})", gameId, MoveParameters.X, MoveParameters.Y);
            return BadRequest("������������ ��������� ��� ���� � ����");
        }
        
        GameShowDTO gameIsReal = await _gameService.GetGameAsync(gameId);
        
        if (gameIsReal == null)
        {
            _logger.LogWarning("���� � ID {GameId} �� �������", gameId);
            return NotFound("���� � ��������� ID �� �������");
        }

        try
        {
            GameShowDTO result = await _gameService.MakeMoveAsync(gameId, MoveParameters);
            Response.Headers.ETag = new Microsoft.Extensions.Primitives.StringValues($"\"{result.Version}\"");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "������ ��� ������� ������� ���");
            throw;
        }
    }
}
