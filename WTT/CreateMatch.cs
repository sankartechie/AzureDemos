
using System;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
//using Microsoft.Azure.Functions.Worker.Sdk;

namespace WTT
{
    public static class CreateMatch
    {
        [Function("CreateMatch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "matches")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            Guid matchId = Guid.NewGuid();
            Guid player1Id = data?.player1Id;
            Guid player2Id = data?.player2Id;
            Guid matchWonBy = data?.matchWonBy;

            string player1Name = data?.player1Name;
            string player2Name = data?.player2Name;
            string player1Nationality = data?.player1Nationality;
            string player2Nationality = data?.player2Nationality;

            string connectionString = "Server=tcp:sankartechiesqlsvr.database.windows.net,1433;Initial Catalog=azuredemosqldb;Persist Security Info=False;User ID=sankartechie;Password=SanTechie@Azure;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var text = @"INSERT INTO Matches (match_id, player1_id, player1_name, player1_nationality, 
                             player2_id, player2_name, player2_nationality, match_won_by) 
                             VALUES (@MatchId, @Player1Id, @Player1Name, @Player1Nationality, 
                                     @Player2Id, @Player2Name, @Player2Nationality, @MatchWonBy)";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    cmd.Parameters.AddWithValue("@MatchId", matchId);
                    cmd.Parameters.AddWithValue("@Player1Id", player1Id);
                    cmd.Parameters.AddWithValue("@Player1Name", player1Name);
                    cmd.Parameters.AddWithValue("@Player1Nationality", player1Nationality);
                    cmd.Parameters.AddWithValue("@Player2Id", player2Id);
                    cmd.Parameters.AddWithValue("@Player2Name", player2Name);
                    cmd.Parameters.AddWithValue("@Player2Nationality", player2Nationality);
                    cmd.Parameters.AddWithValue("@MatchWonBy", matchWonBy);

                    await cmd.ExecuteNonQueryAsync();
                    //log.LogInformation("C# HTTP trigger function added a match.");
                }
            }

            return new OkObjectResult(new { message = "Match created successfully", match_id = matchId });
        }
    }
}
