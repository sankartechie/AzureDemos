
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AzureFunctionsWebApi.Models;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionsWebApi.Services
{
    public class AzureFunctionsClient : IAzureFunctionsClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AzureFunctionsClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["AzureFunctions:BaseUrl"];
        }

        public async Task<bool> CreateMatchAsync(Match match)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/CreateMatch", match);
            return response.IsSuccessStatusCode;
        }

        public async Task<Player> GetPlayerInfoAsync(string playerId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/players/{playerId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Player>();
            }
            return null;
        }
    }
}
