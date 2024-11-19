
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace SportsFunctionsApp.Functions
{
    public static class GetPlayerInfo_sql
    {
        [Function("GetPlayerInfo_SQL")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players_sql/{playerId}")] HttpRequest req,
            string playerId,
            ILogger log)
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
            string sqlConnStr = Configuration["WTT-SQLdbConnStr"];

            using (SqlConnection conn = new SqlConnection(sqlConnStr))
            {
                conn.Open();
                var query = "SELECT * FROM Players WHERE player_id = @PlayerId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlayerId", Guid.Parse(playerId));
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var player = new
                            {
                                player_id = reader["player_id"],
                                name = reader["name"],
                                address = reader["address"],
                                nationality = reader["nationality"],
                                gender = reader["gender"],
                                total_matches_played = reader["total_matches_played"],
                                won = reader["won"],
                                lost = reader["lost"],
                                win_loss_percentage = reader["win_loss_percentage"]
                            };
                            return new OkObjectResult(player);
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                }
            }
        }
    }
}
