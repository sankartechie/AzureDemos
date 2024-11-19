using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace WTT
{
    public static class UpdatePlayerStats_sql
    {

        [Function("UpdatePlayerStats_SQL")]
        public static async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo timer, ILogger log)
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
            //Environment.GetEnvironmentVariable("SqlConnectionString");

            using (SqlConnection conn = new SqlConnection(sqlConnStr))
            {
                conn.Open();

                var getNewMatchesQuery = "SELECT * FROM Matches WHERE Processed = 0";
                using (SqlCommand cmd = new SqlCommand(getNewMatchesQuery, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid matchId = reader.GetGuid(reader.GetOrdinal("match_id"));
                        Guid winnerId = reader.GetGuid(reader.GetOrdinal("match_won_by"));
                        Guid player1Id = reader.GetGuid(reader.GetOrdinal("player1_id"));
                        Guid player2Id = reader.GetGuid(reader.GetOrdinal("player2_id"));
                        Guid loserId = winnerId == player1Id ? player2Id : player1Id;

                        // Update stats for both players
                        string str = "winner: " + winnerId.ToString() + "loser " + loserId.ToString();
                        Debug.Print(str);

                        await UpdatePlayerStatsAsync(conn, winnerId, true);
                        await UpdatePlayerStatsAsync(conn, loserId, false);

                        // Mark match as processed
                        var updateMatchQuery = "UPDATE Matches SET Processed = 1 WHERE match_id = @MatchId";
                        using (SqlCommand updateCmd = new SqlCommand(updateMatchQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@MatchId", matchId);
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                    reader.Close();
                }
                conn.Close();
            }
            //log.LogInformation("Player stats updated.");
        }

        private static async Task UpdatePlayerStatsAsync(SqlConnection conn, Guid playerId, bool won)
        {
            string updateQuery = @"
                UPDATE Players
                SET total_matches_played = total_matches_played + 1,
                    won = won + CASE WHEN @Won = 1 THEN 1 ELSE 0 END,
                    lost = lost + CASE WHEN @Won = 0 THEN 1 ELSE 0 END,
                    win_loss_percentage = 
                        CASE 
                            WHEN total_matches_played > 0 THEN CAST(won AS FLOAT) / CAST(total_matches_played AS FLOAT) * 100
                            ELSE 0
                        END
                WHERE player_id = @PlayerId";

            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@PlayerId", playerId);
                cmd.Parameters.AddWithValue("@Won", won ? 1 : 0);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}