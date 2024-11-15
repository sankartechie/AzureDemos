using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using AzureFunctionsWebApi.Controllers;
using AzureFunctionsWebApi.Models;
using AzureFunctionsWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctionsWebApi.Tests
{
    public class MatchesControllerTests
    {
        private readonly IAzureFunctionsClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IConfiguration _configuration;

        public MatchesControllerTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            //_httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var inMemorySettings = new Dictionary<string, string> { { "AzureFunctions:BaseUrl", "http://localhost:7071/api" } };
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        }

        [Fact]
        public async Task CreateMatch_ReturnsSuccessStatus()
        {
            // Arrange
            //AzureFunctionsWebApi.Models.Match match = < AzureFunctionsWebApi.Models.Match > Models.Match;
            Models.Match match = new Models.Match()
            {
                MatchId = new Guid(),
                Player1Id = new Guid(),
                Player1Name = "Player One",
                Player1Nationality = "Country A",
                Player2Id = new Guid(),
                Player2Name = "Player Two",
                Player2Nationality = "Country B",
                MatchWonBy = new Guid()
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(match), Encoding.UTF8, "application/json");
            //var matchObj = (Models.Match)JsonConvert.DeserializeObject(match);

            // Mock the response
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{\"message\": \"Match created successfully.\"}") });

            var controller = new MatchesController(_httpClient);

            // Act
            IActionResult response = await controller.CreateMatch(match);
            ObjectResult objectResponse = Assert.IsType<ObjectResult>(response);

            // Assert
            Assert.Equal(200, objectResponse.StatusCode);
            var responseString = ""; // await response.Content.ReadAsStringAsync();
            Assert.Contains("Match created successfully", responseString);
        }

        [Fact]
        public async Task CreateMatch_ReturnsInternalServerError()
        {
            // Arrange
            Models.Match match = new Models.Match()
            {
                MatchId = new Guid(),
                Player1Id = new Guid(),
                Player1Name = "Player One",
                Player1Nationality = "Country A",
                Player2Id = new Guid(),
                Player2Name = "Player Two",
                Player2Nationality = "Country B",
                MatchWonBy = new Guid()
            };
            var jsonContent = new StringContent(JsonConvert.SerializeObject(match), Encoding.UTF8, "application/json");

            // Mock a failed response
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            var controller = new MatchesController(_httpClient);

            // Act
            IActionResult response = await controller.CreateMatch(match);
            ObjectResult objectResponse = Assert.IsType<ObjectResult>(response);

            // Assert
            Assert.Equal(500, objectResponse.StatusCode);
        }
    }
}
