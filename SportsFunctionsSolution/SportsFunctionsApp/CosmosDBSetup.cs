using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using SportsFunctionsApp.Models;

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
            //await AddPlayersAsync();
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
            List<Player> players = GeneratePlayers();

            foreach (Player player in players)
            {
                await playerContainer.UpsertItemAsync<Player>(player, new PartitionKey(player.PlayerId.ToString()));
            }

            Console.WriteLine("Players data seeded.");
        }

        private static List<Player> GeneratePlayers()
        {
            var playerNames = new[] { "Alice", "Bob", "Charlie", "Dana", "Eve", "Frank", "Grace", "Hector", "Ivy", "Judy" };
            var nationalities = new[] { "USA", "UK", "Canada", "France", "Germany", "Australia", "India", "Brazil", "Japan", "Mexico" };

            return playerNames.Select((name, index) => new Player
            {
                Id = index.ToString(),
                PlayerId = Guid.NewGuid(),
                Name = name.ToString(),
                Address = $"Address {index + 1}".ToString(),
                Nationality = nationalities[index % nationalities.Length].ToString(),
                Gender = index % 2 == 0 ? "Male" : "Female".ToString(),
                TotalMatchesPlayed = 0,
                Won = 0,
                Lost = 0,
                WinLossPercentage = 0.0
            }).ToList<Player>();
        }

        private static async Task AddMatchesAsync()
        {
            var players = await GetPlayersAsync();
            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                Player player1 = players[random.Next(players.Count)];
                Player player2 = players[random.Next(players.Count)];

                while (player1.PlayerId == player2.PlayerId)
                {
                    player2 = players[random.Next(players.Count)];
                }

                var match = new Match
                {
                    Id = i.ToString(),
                    MatchId = Guid.NewGuid(),
                    Player1Id = player1.PlayerId,
                    Player1Name = player1.Name,
                    Player1Nationality = player1.Nationality,
                    Player2Id = player2.PlayerId,
                    Player2Name = player2.Name,
                    Player2Nationality = player2.Nationality,
                    MatchWonBy = random.Next(2) == 0 ? player1.PlayerId : player2.PlayerId
                };

                await matchContainer.UpsertItemAsync<Match>(match, new PartitionKey(match.MatchId.ToString()));
            }

            Console.WriteLine("Matches data seeded.");
        }

        private static async Task<List<Player>> GetPlayersAsync()
        {
            var query = playerContainer.GetItemQueryIterator<Player>("SELECT * FROM Players");
            var players = new List<Player>();

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
