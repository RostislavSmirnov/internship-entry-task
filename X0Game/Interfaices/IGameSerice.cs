using X0Game.DTOs;

namespace X0Game.Interfaices
{
    public interface IGameSerice
    {
        Task<GameShowDTO> StartNewGameAsync(GameStartModelDTO gameStartModel);

        Task<GameShowDTO> GetGameAsync(int gameId);

        Task<GameShowDTO> MakeMoveAsync(int gdmeId, GameMoveDTO gameParameters);
    }
}
