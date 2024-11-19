
using Newtonsoft.Json;

namespace SportsFunctionsApp.Models
{
    public class Player
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "playerId")]
        public Guid PlayerId { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string Nationality { get; set; }
        public required string Gender { get; set; }
        public int TotalMatchesPlayed { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public double WinLossPercentage { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
