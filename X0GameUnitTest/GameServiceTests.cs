using Moq;
using X0Game.Interfaices;
using X0Game.Models;
using X0Game.Services;
using X0Game.Controllers;
using X0Game.DTOs;
using X0Game.ErrorHandler;
using X0Game.MappingProfile;
using X0Game.Data;
using X0Game.Repositories;
using X0Game.Migrations;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;

namespace X0GameUnitTest
{
    public class GameServiceTests
    {
        private readonly GameService _gameService;
        private readonly Mock<ILogger<GameService>> _MockLoger;
        private readonly IMapper _mapper;
        private readonly Mock<IGameRepository> _gameRepositoryMock;

        public GameServiceTests()
        {
            _MockLoger = new Mock<ILogger<GameService>>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MapperProfile()));
            _mapper = config.CreateMapper();
            _gameService = new GameService(_MockLoger.Object, _mapper, _gameRepositoryMock.Object);
        }


        [Fact]
        public async Task MakeMoveInsertCorectValue()
        {
            Game testGame = new Game
            {
                GameId = 1,
                Field = new List<List<string>>
                {
                    new List<string> { "x", "o", "" },
                    new List<string> { "", "x", "" },
                    new List<string> { "", "", "" }
                },
                FieldSize = 3,
                VictoryCondition = 3,
                CounterOfMoves = 3,
                GameStatus = "InProgress",
                NextPlayer = "o",
                Version = 3
            };

            GameMoveDTO testMove = new GameMoveDTO
            {
                X = 2,
                Y = 3,
                Version = "3"
            };

            _gameRepositoryMock.Setup(repo => repo.GetGameAsync(1)).ReturnsAsync(testGame);
            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>())).Returns(Task.CompletedTask);

            GameShowDTO result = await _gameService.MakeMoveAsync(1, testMove);
            Assert.Equal("o", result.Field[1][2]);
        }


        [Fact]
        public async Task DiogonalWinTrackingRightLeft()
        {
            Game testGame = new Game
            {
                GameId = 1,
                Field = new List<List<string>>
                {
                    new List<string> { "x", "o", "" },
                    new List<string> { "", "x", "" },
                    new List<string> { "", "", "" }
                },
                FieldSize = 3,
                VictoryCondition = 3,
                CounterOfMoves = 3,
                GameStatus = "InProgress",
                NextPlayer = "x",
                Version = 3
            };

            GameMoveDTO testMove = new GameMoveDTO
            {
                X = 3,
                Y = 3,
                Version = "3"
            };

            _gameRepositoryMock.Setup(repo => repo.GetGameAsync(1)).ReturnsAsync(testGame);
            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>())).Returns(Task.CompletedTask);

            GameShowDTO result = await _gameService.MakeMoveAsync(1, testMove);
            Assert.Equal("x wins", result.GameStatus);
        }


        [Fact]
        public async Task DiogonalWinTrackingLeftRight()
        {
            Game testGame = new Game
            {
                GameId = 1,
                Field = new List<List<string>>
                {
                    new List<string> { "", "o", "" },
                    new List<string> { "", "x", "" },
                    new List<string> { "x", "", "" }
                },
                FieldSize = 3,
                VictoryCondition = 3,
                CounterOfMoves = 3,
                GameStatus = "InProgress",
                NextPlayer = "x",
                Version = 3
            };

            GameMoveDTO testMove = new GameMoveDTO
            {
                X = 1,
                Y = 3,
                Version = "3"
            };

            _gameRepositoryMock.Setup(repo => repo.GetGameAsync(1)).ReturnsAsync(testGame);
            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>())).Returns(Task.CompletedTask);

            GameShowDTO result = await _gameService.MakeMoveAsync(1, testMove);
            Assert.Equal("x wins", result.GameStatus);
        }


        [Fact]
        public async Task VertikalWinTracking()
        {
            Game testGame = new Game
            {
                GameId = 1,
                Field = new List<List<string>>
                {
                    new List<string> { "o", "x", "" },
                    new List<string> { "", "x", "" },
                    new List<string> { "", "", "" }
                },
                FieldSize = 3,
                VictoryCondition = 3,
                CounterOfMoves = 3,
                GameStatus = "InProgress",
                NextPlayer = "x",
                Version = 3
            };

            GameMoveDTO testMove = new GameMoveDTO
            {
                X = 3,
                Y = 2,
                Version = "3"
            };

            _gameRepositoryMock.Setup(repo => repo.GetGameAsync(1)).ReturnsAsync(testGame);
            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>())).Returns(Task.CompletedTask);

            GameShowDTO result = await _gameService.MakeMoveAsync(1, testMove);
            Assert.Equal("x wins", result.GameStatus);
        }
        

        [Fact]
        public async Task DrawTracking()
        {
            Game testGame = new Game
            {
                GameId = 1,
                Field = new List<List<string>>
                {
                     new List<string> { "x", "o", "x" },
                     new List<string> { "x", "",  "o" }, 
                     new List<string> { "o", "x", "o" }
                },
                FieldSize = 3,
                VictoryCondition = 3,
                CounterOfMoves = 8,
                GameStatus = "InProgress",
                NextPlayer = "o",
                Version = 8
            };

            GameMoveDTO testMove = new GameMoveDTO
            {
                X = 2,
                Y = 2,
                Version = "8"
            };

            _gameRepositoryMock.Setup(repo => repo.GetGameAsync(1)).ReturnsAsync(testGame);
            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>())).Returns(Task.CompletedTask);

            GameShowDTO result = await _gameService.MakeMoveAsync(1, testMove);
            Assert.Equal("Draw", result.GameStatus);
        }


        [Fact]
        public async Task MoveOnNotEmptyCell()
        {
            Game testGame = new Game
            {
                GameId = 1,
                Field = new List<List<string>>
                {
                    new List<string> { "o", "x", "" },
                    new List<string> { "x", "x", "o" },
                    new List<string> { "o", "o", "x" }
                },
                FieldSize = 3,
                VictoryCondition = 3,
                CounterOfMoves = 8,
                GameStatus = "InProgress",
                NextPlayer = "o",
                Version = 3
            };

            GameMoveDTO testMove = new GameMoveDTO
            {
                X = 1,
                Y = 1,
                Version = "3"
            };

            _gameRepositoryMock.Setup(repo => repo.GetGameAsync(1)).ReturnsAsync(testGame);
            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>())).Returns(Task.CompletedTask);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _gameService.MakeMoveAsync(1, testMove);
            });

            Assert.Equal("Эта клетка уже занята. Выберите другую.", exception.Message);
        }


        [Fact]
        public async Task MakeMoveAsync_WhenConcurrencyExceptionOccurs_ReturnsLatestGameStatet()
        {
            int gameId = 1;
            GameMoveDTO moveWithOldVersion = new GameMoveDTO 
            { X = 2,
              Y = 2,
              Version = "9"
            };

            Game gameInDb = new Game
            {
                GameId = gameId,
                Field = new List<List<string>> { new List<string> { "x", "" }, new List<string> { "", "" } },
                GameStatus = "InProgress",
                NextPlayer = "o",
                Version = 10 
            };

            _gameRepositoryMock.Setup(r => r.GetGameAsync(gameId))
            .ReturnsAsync(new Game { GameId = gameId, Version = 10 });

            _gameRepositoryMock.Setup(r => r.SaveMove(It.IsAny<Game>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            _gameRepositoryMock.Setup(r => r.GetGameAsync(gameId))
                .ReturnsAsync(gameInDb);

            GameShowDTO result = await _gameService.MakeMoveAsync(gameId, moveWithOldVersion);

            Assert.Equal(gameInDb.Version.ToString(), result.Version);
        }
    }
}
