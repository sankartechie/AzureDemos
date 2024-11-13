
namespace AzureFunctionsWebApi.Models
{
    public class Player
    {
        public Guid PlayerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public int TotalMatchesPlayed { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        public double WinLossPercentage { get; set; }
    }
}
