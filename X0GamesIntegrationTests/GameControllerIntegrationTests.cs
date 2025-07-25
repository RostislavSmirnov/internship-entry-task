using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Query.Internal;
using X0Game.DTOs;
using X0Game.Controllers;
using Newtonsoft.Json;
using System.Net;


namespace X0GamesIntegrationTests
{
    public class GameControllerIntegrationTests : IClassFixture<WebApplicationFactory>
    {
        private readonly HttpClient _client;

        public GameControllerIntegrationTests(WebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task StartGame_Return201Created() 
        {
            GameStartModelDTO testStartModel = new GameStartModelDTO
            {
                FieldSize = 3,
                VictoryCondition = 3,
                NextPlayer = "x"
            };
            StringContent content = new StringContent(JsonConvert.SerializeObject(testStartModel), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/Game", content);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("gameId", responseContent);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }


        [Fact]
        public async Task MakeMove_WithInvalidJson_ReturnsBadRequest()
        {
            StringContent invalidJsonContent = new StringContent("{ \"x\": 1, \"y\": 2, \"version\": ", Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("/Game/123/moves", invalidJsonContent);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task GetGameReturn404() 
        {
            int IdForSearch = 9999;
            HttpResponseMessage response = await _client.GetAsync($"/api/Game/{IdForSearch}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task MakeMove_WithDuplicateRequest_IsIdempotentAndReturnsOk()
        {
            GameStartModelDTO startGameDto = new GameStartModelDTO
            {
                FieldSize = 3,
                VictoryCondition = 3,
                NextPlayer = "x"
            };
            StringContent startGameContent = new StringContent(JsonConvert.SerializeObject(startGameDto), Encoding.UTF8, "application/json");
            HttpResponseMessage createResponse = await _client.PostAsync("/Game", startGameContent);
            createResponse.EnsureSuccessStatusCode();

            GameShowDTO? createdGame = JsonConvert.DeserializeObject<GameShowDTO>(await createResponse.Content.ReadAsStringAsync());
            GameMoveDTO moveDto = new GameMoveDTO
            {
                X = 1,
                Y = 1,
                Version = createdGame.Version
            };
            StringContent moveContent = new StringContent(JsonConvert.SerializeObject(moveDto), Encoding.UTF8, "application/json");

            HttpResponseMessage firstMoveResponse = await _client.PostAsync($"/Game/{createdGame.GameId}/moves", moveContent);
            HttpResponseMessage secondMoveResponse = await _client.PostAsync($"/Game/{createdGame.GameId}/moves", moveContent);

            Assert.Equal(HttpStatusCode.OK, firstMoveResponse.StatusCode);
            GameShowDTO? firstMoveResult = JsonConvert.DeserializeObject<GameShowDTO>(await firstMoveResponse.Content.ReadAsStringAsync());
            Assert.NotEqual(createdGame.Version, firstMoveResult.Version);
            Assert.Equal("x", firstMoveResult.Field[0][0]);

            Assert.Equal(HttpStatusCode.OK, secondMoveResponse.StatusCode);
            GameShowDTO? secondMoveResult = JsonConvert.DeserializeObject<GameShowDTO>(await secondMoveResponse.Content.ReadAsStringAsync());

            Assert.Equal(firstMoveResult.Version, secondMoveResult.Version);
            Assert.Equal(firstMoveResult.CounterOfMoves, secondMoveResult.CounterOfMoves); 
            Assert.Equal(firstMoveResponse.Headers.ETag, secondMoveResponse.Headers.ETag);
        }
    }
}
