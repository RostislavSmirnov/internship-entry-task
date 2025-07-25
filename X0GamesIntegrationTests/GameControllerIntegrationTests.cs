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
        public async Task MakeMove_WithDuplicateRequest_IsIdempotentAndReturnsOk()
        {
            var startGameDto = new GameStartModelDTO
            {
                FieldSize = 3,
                VictoryCondition = 3,
                NextPlayer = "x"
            };
            var startGameContent = new StringContent(JsonSerializer.Serialize(startGameDto), Encoding.UTF8, "application/json");
            var createResponse = await _client.PostAsync("/Game", startGameContent);
            createResponse.EnsureSuccessStatusCode();

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(createResponseContent);
            var gameId = jsonDoc.RootElement.GetProperty("gameId").GetInt32();
            var initialVersion = jsonDoc.RootElement.GetProperty("version").GetString();

            var moveDto = new GameMoveDTO
            {
                X = 1,
                Y = 1,
                Version = initialVersion
            };
            var moveContent = new StringContent(JsonSerializer.Serialize(moveDto), Encoding.UTF8, "application/json");

            var firstMoveResponse = await _client.PostAsync($"/Game/{gameId}/moves", moveContent);

            var secondMoveContent = new StringContent(JsonSerializer.Serialize(moveDto), Encoding.UTF8, "application/json");
            var secondMoveResponse = await _client.PostAsync($"/Game/{gameId}/moves", secondMoveContent);

            Assert.Equal(HttpStatusCode.OK, firstMoveResponse.StatusCode);
            var firstMoveResultContent = await firstMoveResponse.Content.ReadAsStringAsync();
            using var firstMoveJsonDoc = JsonDocument.Parse(firstMoveResultContent);
            var firstMoveVersion = firstMoveJsonDoc.RootElement.GetProperty("version").GetString();
            Assert.NotEqual(initialVersion, firstMoveVersion); 

            Assert.Equal(HttpStatusCode.OK, secondMoveResponse.StatusCode); 

            Assert.Equal(firstMoveResponse.Headers.ETag, secondMoveResponse.Headers.ETag);
        }
    }
}
