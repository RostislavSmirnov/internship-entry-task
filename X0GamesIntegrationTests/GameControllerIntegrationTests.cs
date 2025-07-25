using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Query.Internal;
using X0Game.DTOs;
using X0Game.Controllers;
using System.Net;
using X0Game;
using System.Text.Json;

namespace X0GamesIntegrationTests
{
    public class GameControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions; 

        public GameControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
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
            StringContent content = new StringContent(JsonSerializer.Serialize(testStartModel), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/Game", content);

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
            HttpResponseMessage response = await _client.GetAsync($"/Game/{IdForSearch}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task MakeMove_WithValidMove_ReturnsOkAndUpdatedState()
        {
            GameStartModelDTO startGameDto = new GameStartModelDTO { FieldSize = 3, VictoryCondition = 3, NextPlayer = "x" };
            StringContent startGameContent = new StringContent(JsonSerializer.Serialize(startGameDto), Encoding.UTF8, "application/json");
            HttpResponseMessage createResponse = await _client.PostAsync("/Game", startGameContent);
            createResponse.EnsureSuccessStatusCode();
            string createResponseContent = await createResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(createResponseContent);
            int gameId = jsonDoc.RootElement.GetProperty("gameId").GetInt32();
            string? initialVersion = jsonDoc.RootElement.GetProperty("version").GetString();


            GameMoveDTO moveDto = new GameMoveDTO { X = 1, Y = 1, Version = initialVersion };
            StringContent moveContent = new StringContent(JsonSerializer.Serialize(moveDto), Encoding.UTF8, "application/json");


            HttpResponseMessage moveResponse = await _client.PostAsync($"/Game/{gameId}/moves", moveContent);


            Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
            string moveResultContent = await moveResponse.Content.ReadAsStringAsync();
            GameShowDTO? moveResult = JsonSerializer.Deserialize<GameShowDTO>(moveResultContent, _jsonOptions);


            Assert.Equal("x", moveResult.Field[0][0]);
            Assert.Equal(1, moveResult.CounterOfMoves);
            Assert.Equal("o", moveResult.NextPlayer);
        }
    }
}
