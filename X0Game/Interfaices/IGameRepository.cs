using X0Game.Models;

namespace X0Game.Interfaices
{
    public interface IGameRepository
    {
        Task<int> StartNewGameAsync(Game gameStartModel);
        Task<Game> GetGameAsync(int gameId);

        Task SaveMove(Game savedGameMove);
    }
}
