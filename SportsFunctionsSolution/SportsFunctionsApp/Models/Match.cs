
using Newtonsoft.Json;

namespace SportsFunctionsApp.Models
{
    public class Match
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "matchId")]
        public Guid MatchId { get; set; }
        public Guid Player1Id { get; set; }
        public required string Player1Name { get; set; }
        public required string Player1Nationality { get; set; }
        public Guid Player2Id { get; set; }
        public required string Player2Name { get; set; }
        public required string Player2Nationality { get; set; }
        public Guid MatchWonBy { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
