using Anekdotify.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/seeder")]
    [Authorize(Roles = "Admin")]
    public class JokeSeederController : ControllerBase
    {
        private readonly IJokeSeederService _jokeSeeder;
        public JokeSeederController(IJokeSeederService jokeSeeder)
        {
            _jokeSeeder = jokeSeeder;
        }

        [HttpPost("seed/{count:int}")]
        public async Task<IActionResult> SeedJokes([FromRoute] int count)
        {
            try
            {
                await _jokeSeeder.SeedAsync(count);
                return Ok(new { Message = $"{count} jokes seeded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while seeding jokes.", Error = ex.Message });
            }
        }
    }
}
