
using System.Threading.Tasks;
using AzureFunctionsWebApi.Models;

namespace AzureFunctionsWebApi.Services
{
    public interface IAzureFunctionsClient
    {
        Task<bool> CreateMatchAsync(Match match);
        Task<Player> GetPlayerInfoAsync(string playerId);
    }
}
