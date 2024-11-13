
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace SportsFunctionsApp.Functions
{
    public static class GetPlayerInfo
    {
        [Function("GetPlayerInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "players/{playerId}")] HttpRequest req,
            string playerId,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            using (SqlConnection conn = new SqlConnection(connectionString))
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
