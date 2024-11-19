
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using SportsFunctionsApp.Utilities;
using SportsFunctionsApp.Models;
using Microsoft.Azure.Cosmos;
//using System.ComponentModel;
//using Microsoft.Azure.Functions.Worker.Sdk;
//using System.Configuration;

namespace WTT
{
    public static class CreateMatch_Cosmos
    {
        private static readonly CosmosDbHelper _cosmosDbHelper;

        static CreateMatch_Cosmos()
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

        [Function("CreateMatch_Cosmos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "matches_cosmos")] HttpRequest req,
            ILogger log)
        {
            Container matchcontainer;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Match match = JsonConvert.DeserializeObject<Match>(requestBody);

            if (match == null)
            {
                return new BadRequestObjectResult("Invalid match data.");
            }

            Guid matchId = Guid.NewGuid();
            Guid player1Id = match.Player1Id;
            Guid player2Id = match.Player2Id;
            Guid matchWonBy = match.MatchWonBy;

            string player1Name = match.Player1Name;
            string player2Name = match.Player2Name;
            string player1Nationality = match.Player1Nationality;
            string player2Nationality = match.Player2Nationality;

            matchcontainer = _cosmosDbHelper.GetContainer("Matches");
            await matchcontainer.CreateItemAsync(match, new PartitionKey(match.MatchId.ToString()));

            return new OkObjectResult(new { message = "Match created successfully", match.MatchId });
        }
    }
}