
using System.Threading.Tasks;
using AzureFunctionsWebApi.Models;
using AzureFunctionsWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctionsWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly IAzureFunctionsClient _azureFunctionsClient;

        public MatchesController(IAzureFunctionsClient azureFunctionsClient)
        {
            _azureFunctionsClient = azureFunctionsClient;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] Match match)
        {
            var success = await _azureFunctionsClient.CreateMatchAsync(match);
            if (success)
            {
                return Ok("Match created successfully.");
            }
            return StatusCode(500, "Failed to create match.");
        }
    }
}
