
namespace SportsFunctionsApp.Models
{
    public class Match
    {
        public Guid MatchId { get; set; }
        public Guid Player1Id { get; set; }
        public required string Player1Name { get; set; }
        public required string Player1Nationality { get; set; }
        public Guid Player2Id { get; set; }
        public required string Player2Name { get; set; }
        public required string Player2Nationality { get; set; }
        public Guid MatchWonBy { get; set; }
    }
}
