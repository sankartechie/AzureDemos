using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace SportsFunctionsApp
{
    public class CosmosDBSetup
    {
        private static string cosmosDBep;
        private static string cosmosDBpk;
        private static CosmosClient cosmosClient;
        private static Database database;
        private static Microsoft.Azure.Cosmos.Container playerContainer;
        private static Microsoft.Azure.Cosmos.Container matchContainer;

        public async Task CosmosDBsetup()
        {
            var azureCredentialOptions = new DefaultAzureCredentialOptions();
            var credential = new DefaultAzureCredential(azureCredentialOptions);
            var AzKeyVaultUri = Environment.GetEnvironmentVariable("AzKeyVaultUri");

            // Create a SecretClient
            var secretClient = new SecretClient(new Uri(AzKeyVaultUri.ToString()), credential);
            var Configuration = new ConfigurationBuilder()
                    .AddAzureKeyVault(new Uri(AzKeyVaultUri),
                        new DefaultAzureCredential())
                    .Build();
            cosmosDBpk = Configuration["SanWTTCosmosDB-PrimaryKey"];
            cosmosDBep = Environment.GetEnvironmentVariable("CosmosDB_Endpoint"); 

            cosmosClient = new CosmosClient(cosmosDBep, cosmosDBpk);

            // Create Database and Containers
            await CreateDatabaseAsync();
            await CreateContainersAsync();

            // Seed Players and Matches data
            await AddPlayersAsync();
            await AddMatchesAsync();

            Console.WriteLine("Database setup and data seeding complete!");
        }

        private static async Task CreateDatabaseAsync()
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync("WTT_SportsDB");
            Console.WriteLine("Database 'WTT_SportsDB' created (or existed already).");
        }

        private static async Task CreateContainersAsync()
        {
            playerContainer = await database.CreateContainerIfNotExistsAsync("Players", "/playerId");
            Console.WriteLine("Container 'Players' created (or existed already).");

            matchContainer = await database.CreateContainerIfNotExistsAsync("Matches", "/matchId");
            Console.WriteLine("Container 'Matches' created (or existed already).");
        }

        private static async Task AddPlayersAsync()
        {
            var players = GeneratePlayers();

            foreach (var player in players)
            {
                await playerContainer.UpsertItemAsync(player, player.name);
            }

            Console.WriteLine("Players data seeded.");
        }

        private static List<dynamic> GeneratePlayers()
        {
            var playerNames = new[] { "Alice", "Bob", "Charlie", "Dana", "Eve", "Frank", "Grace", "Hector", "Ivy", "Judy" };
            var nationalities = new[] { "USA", "UK", "Canada", "France", "Germany", "Australia", "India", "Brazil", "Japan", "Mexico" };

            return playerNames.Select((name, index) => new
            {
                playerId = Guid.NewGuid(),
                name = name,
                address = $"Address {index + 1}",
                nationality = nationalities[index % nationalities.Length],
                gender = index % 2 == 0 ? "Male" : "Female",
                totalMatchesPlayed = 0,
                won = 0,
                lost = 0,
                winLossPercentage = 0.0
            }).ToList<dynamic>();
        }

        private static async Task AddMatchesAsync()
        {
            var players = await GetPlayersAsync();
            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                var player1 = players[random.Next(players.Count)];
                var player2 = players[random.Next(players.Count)];

                while (player1.playerId == player2.playerId)
                {
                    player2 = players[random.Next(players.Count)];
                }

                var match = new
                {
                    matchId = Guid.NewGuid(),
                    player1Id = player1.playerId,
                    player1Name = player1.name,
                    player1Nationality = player1.nationality,
                    player2Id = player2.playerId,
                    player2Name = player2.name,
                    player2Nationality = player2.nationality,
                    matchWonBy = random.Next(2) == 0 ? player1.playerId : player2.playerId
                };

                await matchContainer.UpsertItemAsync(match);
            }

            Console.WriteLine("Matches data seeded.");
        }

        private static async Task<List<dynamic>> GetPlayersAsync()
        {
            var query = playerContainer.GetItemQueryIterator<dynamic>("SELECT * FROM Players");
            var players = new List<dynamic>();

            while (query.HasMoreResults)
            {
                foreach (var player in await query.ReadNextAsync())
                {
                    players.Add(player);
                }
            }

            return players;
        }
    }
}
