using X0Game.Models;
using X0Game.Data;
using X0Game.Interfaices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace X0Game.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly GameDbContext _context;
        private readonly ILogger<GameRepository> _logger;
        public GameRepository(GameDbContext context, ILogger<GameRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<int> StartNewGameAsync(Game gameStartModel)
        {
            try
            {
                await _context.Games.AddAsync(gameStartModel);
                await _context.SaveChangesAsync();
                _context.Entry(gameStartModel).State = EntityState.Detached;
                return gameStartModel.GameId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении новой игры в репозитории: {@GameStartModel}", gameStartModel);
                throw;
            }
        }


        public async Task<Game> GetGameAsync(int gameId)
        {
            try
            {
                Game game = await _context.Games.FindAsync(gameId);
                if (game == null)
                {
                    _logger.LogWarning("Игра с ID {GameId} не найдена в репозитории", gameId);
                }
                return game;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении игры с ID {GameId} из репозитория", gameId);
                throw;
            }
        }

        public async Task SaveMove(Game savedGameMove) 
        {
            _context.Entry(savedGameMove).Property(g => g.Field).IsModified = true;
            await _context.SaveChangesAsync();
        }
    }
}