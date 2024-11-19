using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SportsFunctionsApp.Utilities;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace WTT
{
    public class CdcPoller
    {
        private readonly Container _matchesContainer;
        private readonly Container _playersContainer;
        private readonly Container _leaseContainer;
        private readonly CosmosDbHelper _cosmosDbHelper;

        public CdcPoller()
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

            _matchesContainer = _cosmosDbHelper.GetContainer("Matches");
            _playersContainer = _cosmosDbHelper.GetContainer("Players");
            _leaseContainer = _cosmosDbHelper.GetContainer("leases");
        }

        [Function("CdcPoller")]
        public async Task RunAsync([TimerTrigger("*/30 * * * * *")] TimerInfo timer, FunctionContext context)
        {
            var logger = context.GetLogger("CdcPoller");
            logger.LogInformation($"CDC Poller executed at: {DateTime.UtcNow}");

            try
            {
                var iterator = _matchesContainer.GetChangeFeedIterator<dynamic>(
                    ChangeFeedStartFrom.Now(), // Start reading from the latest changes
                    ChangeFeedMode.Incremental // Incremental mode for efficiency
                );

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    foreach (var match in response)
                    {
                        string winnerId = match["match_won_by"];
                        string player1Id = match["player1_id"];
                        string player2Id = match["player2_id"];

                        // Update the winner's stats
                        await UpdatePlayerStats(winnerId, won: true);

                        // Update the loser's stats
                        string loserId = winnerId == player1Id ? player2Id : player1Id;
                        await UpdatePlayerStats(loserId, won: false);

                        logger.LogInformation($"Processed match: {match["match_id"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing change feed: {ex.Message}");
            }
        }

        private async Task UpdatePlayerStats(string playerId, bool won)
        {
            try
            {
                // Read player data
                var playerResponse = await _playersContainer.ReadItemAsync<dynamic>(playerId, new PartitionKey(playerId));
                var player = playerResponse.Resource;

                // Update stats
                player["total_matches_played"] += 1;
                if (won)
                    player["won"] += 1;
                else
                    player["lost"] += 1;

                player["win_loss_percentage"] = Math.Round((double)player["won"] / player["total_matches_played"] * 100, 2);

                // Upsert updated player data
                await _playersContainer.UpsertItemAsync(player, new PartitionKey(playerId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating player stats for Player ID {playerId}: {ex.Message}");
            }
        }
    }
}