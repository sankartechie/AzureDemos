
using System.Threading.Tasks;
using AzureFunctionsWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctionsWebApi.Controllers
{
    [Route("wttapi/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly IAzureFunctionsClient _azureFunctionsClient;

        public PlayersController(IAzureFunctionsClient azureFunctionsClient)
        {
            _azureFunctionsClient = azureFunctionsClient;
        }

        [HttpGet("{playerId}")]
        public async Task<IActionResult> GetPlayerInfo(string playerId)
        {
            var player = await _azureFunctionsClient.GetPlayerInfoAsync(playerId);
            if (player != null)
            {
                return Ok(player);
            }
            return NotFound("Player not found.");
        }
    }
}
