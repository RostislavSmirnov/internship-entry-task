using X0Game.DTOs;
using X0Game.Models;
using X0Game.Interfaices;
using AutoMapper;
using Serilog;
using Npgsql.Internal.Postgres;
using Microsoft.EntityFrameworkCore;

namespace X0Game.Services
{
    public class GameService : IGameSerice
    {
        private readonly ILogger<GameService> _logger;
        private readonly IMapper _mapper;
        private readonly IGameRepository _gameRepository;
        public GameService(ILogger<GameService> logger, IMapper mapper, IGameRepository gameRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _gameRepository = gameRepository;
        }


        public async Task<GameShowDTO> StartNewGameAsync(GameStartModelDTO gameStartModel)
        {
            _logger.LogInformation("Поступление запроса начала игры {@GameStartParameters} в сервисный слой", gameStartModel);

            Game game = _mapper.Map<Game>(gameStartModel);
            game.Field = Enumerable.Range(0, game.FieldSize)
                .Select(_ => Enumerable.Repeat(string.Empty, game.FieldSize).ToList())
                .ToList();
            game.GameStatus = "InProgress";
            int gameId = await _gameRepository.StartNewGameAsync(game);
            Game createdGameWithVersion = await _gameRepository.GetGameAsync(gameId);
            
            if (createdGameWithVersion == null)
            {
                throw new InvalidOperationException("Не удалось получить игру сразу после создания.");
            }

            return _mapper.Map<GameShowDTO>(createdGameWithVersion);
        }


        public async Task<GameShowDTO> GetGameAsync(int gameId)
        {
            _logger.LogInformation("Поступление запроса на получение игры с ID: {GameId}", gameId);
            Game game = await _gameRepository.GetGameAsync(gameId);
            
            if (game == null)
            {
                _logger.LogWarning("Игра с ID {GameId} не найдена", gameId);
                return null;
            }

            GameShowDTO gameShowDTO = _mapper.Map<GameShowDTO>(game);
            return gameShowDTO;
        }


        public async Task<GameShowDTO> MakeMoveAsync(int gameId, GameMoveDTO gameParameters)
        {
            int rowIndex = gameParameters.X - 1;
            int columnIndex = gameParameters.Y - 1;

            Game game = await _gameRepository.GetGameAsync(gameId);

            uint clientVersion = uint.Parse(gameParameters.Version);
            if (game.Version != clientVersion)
            {
                _logger.LogWarning("Попытка сделать ход в игре {GameId} с устаревшей версией. Возвращаем текущее состояние", gameId);
                return _mapper.Map<GameShowDTO>(game);
            }

            if (game.GameStatus != "InProgress") 
            {
                _logger.LogWarning("Попытка сделать ход в уже завершенной игре {GameId}", gameId);
                throw new InvalidOperationException($"Игра уже завершена со статусом: {game.GameStatus}");
            }
            
            if (!string.IsNullOrEmpty(game.Field[rowIndex][columnIndex]))
            {
                _logger.LogWarning("Попытка сделать ход в уже занятую клетку: ({X}, {Y}) в игре с ID {GameId}", gameParameters.X, gameParameters.Y, gameId);
                throw new InvalidOperationException("Эта клетка уже занята. Выберите другую.");
            }
            
            string actualNextPlayer = game.NextPlayer;
            
            if ((game.CounterOfMoves +1) % 3 == 0 )
            {
                Random chanceToTrap = new Random();
                int chanceToTrapValue = chanceToTrap.Next(1, 101);
                if (chanceToTrapValue <= 10)
                {
                    actualNextPlayer = actualNextPlayer == "x" ? "o" : "x";
                    _logger.LogInformation("Смена игрока который сделал ход, теперь {Player} делает ход", actualNextPlayer);
                }
            }
            game.Field[rowIndex][columnIndex] = actualNextPlayer;
            game.CounterOfMoves++;
            await GameAnalyser(game, actualNextPlayer);
            
            if (game.GameStatus == "InProgress")
            {
                game.NextPlayer = (game.NextPlayer == "x") ? "o" : "x";
            }

            try
            {
                await _gameRepository.SaveMove(game);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении хода в игре с ID {GameId} из-за конфликта версий.", gameId);
                Game actualGame = await _gameRepository.GetGameAsync(gameId);
                throw;
            }

            return _mapper.Map<GameShowDTO>(game);
        }


        private async Task GameAnalyser(Game game, string actualNextPlayer)
        {
            int size = game.FieldSize;
            int victory = game.VictoryCondition;
            string player = actualNextPlayer;

            //Проверка по горизонтали
            for (int row = 0; row < size; row++)
            {
                int count = 0;
                for (int col = 0; col < size; col++)
                {
                    if (game.Field[row][col] == player)
                    {
                        count++;
                        if (count >= victory)
                        {
                            game.GameStatus = $"{player} wins";
                            _logger.LogInformation("Игра {GameId}: победа игрока {Player} по горизонтали", game.GameId, player);
                            await _gameRepository.SaveMove(game);
                            return;
                        }
                    }
                    else
                    {
                        count = 0;
                    }
                }
            }

            //Проверка по вертикали
            for (int col = 0; col < size; col++)
            {
                int count = 0;
                for (int row = 0; row < size; row++)
                {
                    if (game.Field[row][col] == player)
                    {
                        count++;
                        if (count >= victory)
                        {
                            game.GameStatus = $"{player} wins";
                            _logger.LogInformation("Игра {GameId}: победа игрока {Player} по вертикали", game.GameId, player);
                            await _gameRepository.SaveMove(game);
                            return;
                        }
                    }
                    else
                    {
                        count = 0;
                    }
                }
            }

            //Проверка по диагонали
            for (int row = 0; row <= size - victory; row++)
            {
                for (int col = 0; col <= size - victory; col++)
                {
                    int count = 0;
                    for (int i = 0; i < victory; i++)
                    {
                        if (game.Field[row + i][col + i] == player)
                        {
                            count++;
                            if (count >= victory)
                            {
                                game.GameStatus = $"{player} wins";
                                _logger.LogInformation("Игра {GameId}: победа игрока {Player} по диагонали", game.GameId, player);
                                await _gameRepository.SaveMove(game);
                                return;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            //Проверка по диагонали в другую сторону
            for (int row = 0; row <= size - victory; row++)
            {
                for (int col = victory - 1; col < size; col++)
                {
                    int count = 0;
                    for (int i = 0; i < victory; i++)
                    {
                        if (game.Field[row + i][col - i] == player)
                        {
                            count++;
                            if (count >= victory)
                            {
                                game.GameStatus = $"{player} wins";
                                _logger.LogInformation("Игра {GameId}: победа игрока {Player} по диагонали", game.GameId, player);
                                await _gameRepository.SaveMove(game);
                                return;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            //Проверка на ничью
            if (game.CounterOfMoves >= size * size)
            {
                game.GameStatus = "Draw";
                _logger.LogInformation("Игра {GameId} завершена вничью", game.GameId);
                await _gameRepository.SaveMove(game);
            }
        }

    }
}
