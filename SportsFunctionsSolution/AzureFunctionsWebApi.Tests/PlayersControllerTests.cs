using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using AzureFunctionsWebApi.Controllers;
using Newtonsoft.Json;
using AzureFunctionsWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctionsWebApi.Tests
{
    public class PlayersControllerTests
    {
        private readonly IAzureFunctionsClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IConfiguration _configuration;

        public PlayersControllerTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            //_httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var inMemorySettings = new Dictionary<string, string> { { "AzureFunctions:BaseUrl", "http://localhost:7071/api" } };
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }
            
        [Fact]
        public async Task GetPlayerInfo_ReturnsPlayerData()
        {
            // Arrange
            var playerId = "player-guid-here";
            var expectedPlayer = new
            {
                playerId = playerId,
                name = "Player Name",
                address = "123 Example St.",
                nationality = "Country",
                gender = "Male",
                totalMatchesPlayed = 50,
                won = 30,
                lost = 20,
                winLossPercentage = 60.0
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(expectedPlayer))
                });

            var controller = new PlayersController(_httpClient);

            // Act
            IActionResult response = await controller.GetPlayerInfo(playerId);
            ObjectResult objectResponse = Assert.IsType<ObjectResult>(response);

            // Assert
            Assert.Equal(200, objectResponse.StatusCode);
            var responseString = ""; //await response.Content;
            Assert.Contains("Player Name", responseString);
        }

        [Fact]
        public async Task GetPlayerInfo_ReturnsNotFound()
        {
            // Arrange
            var playerId = "nonexistent-player-id";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            var controller = new PlayersController(_httpClient);

            // Act
            IActionResult response = await controller.GetPlayerInfo(playerId);
            ObjectResult objectResponse = Assert.IsType<ObjectResult>(response);

            // Assert
            Assert.Equal(404, objectResponse.StatusCode);
        }
    }
}
