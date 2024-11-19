
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using SportsFunctionsApp.Utilities;
using Microsoft.Azure.Cosmos;

namespace SportsFunctionsApp.Functions
{
    public static class GetPlayerInfo_Cosmos
    {
        private static readonly CosmosDbHelper _cosmosDbHelper;
        static GetPlayerInfo_Cosmos()
        {
            var azureCredentialOptions = new DefaultAzureCredentialOptions();
            var credential = new DefaultAzureCredential(azureCredentialOptions);
            var AzKeyVaultUri = Environment.GetEnvironmentVariable("AzKeyVaultUri");

            // Create a SecretClient
            var secretClient = new SecretClient(new Uri(AzKeyVaultUri), credential);
            var Configuration = new ConfigurationBuilder()
                    .AddAzureKeyVault(new Uri(AzKeyVaultUri),
                        new DefaultAzureCredential())
                    .Build();
            string cosmosDBpk = Configuration["SanWTTCosmosDB-PrimaryKey"];

            _cosmosDbHelper = new CosmosDbHelper(Environment.GetEnvironmentVariable("CosmosDB_Endpoint"), cosmosDBpk, "WTT_SportsDB");
        }

        [Function("GetPlayerInfo_Cosmos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players/{playerId}")] HttpRequest req,
            string playerId, ILogger log)
        {
            Container playerContainer;
            log.LogInformation($"Fetching details for player with ID: {playerId}");

            try
            {
                playerContainer = _cosmosDbHelper.GetContainer("Player");

                var playerResponse = await playerContainer.ReadItemAsync<dynamic>(playerId, new PartitionKey(playerId));
                var player = playerResponse.Resource;

                return new OkObjectResult(player);
            }
            catch (Exception ex)
            {
                log.LogError($"Error fetching player info: {ex.Message}");
                return new NotFoundObjectResult(new { error = "Player not found" });
            }
        }
    }
}
