using Anekdotify.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/seeder")]
    [Authorize(Roles = "Admin")]
    public class JokeSeederController : ControllerBase
    {
        private readonly IJokeSeederService _jokeSeeder;
        private readonly ILogger<JokeSeederController> _logger;

        public JokeSeederController(IJokeSeederService jokeSeeder, ILogger<JokeSeederController> logger)
        {
            _jokeSeeder = jokeSeeder;
            _logger = logger;
        }

        [HttpPost("seed/{count:int}")]
        public async Task<IActionResult> SeedJokes([FromRoute] int count)
        {
            try
            {
                _logger.LogInformation("Seeding {Count} jokes started by user {User}", count, User.Identity?.Name ?? "Unknown");
                await _jokeSeeder.SeedAsync(count);
                _logger.LogInformation("{Count} jokes seeded successfully.", count);
                return Ok(new { Message = $"{count} jokes seeded successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding {Count} jokes.", count);
                return StatusCode(500, new { Message = "An error occurred while seeding jokes.", Error = ex.Message });
            }
        }
    }
}
