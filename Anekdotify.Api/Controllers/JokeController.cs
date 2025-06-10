using System.Security.Claims;
using Anekdotify.Api.Mappers;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [Route("api/joke")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        private readonly IJokeService _jokeService;
        private readonly IUserSavedJokeRepository _userSavedJokeRepo;
        public JokeController(IJokeService jokeService, IUserSavedJokeRepository userSavedJokeRepo)
        {
            _jokeService = jokeService;

            _userSavedJokeRepo = userSavedJokeRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetJokesAsync([FromQuery] JokesQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var jokeDTOs = await _jokeService.GetAllJokesAsync(query);
            if (jokeDTOs == null)
            {
                return NotFound("No jokes found.");
            }
            return Ok(jokeDTOs);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJokeById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var joke = await _jokeService.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            return Ok(joke);
        }
        

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateJokeAsync([FromBody] JokeCreateDTO jokeCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeCreateDTO == null || string.IsNullOrWhiteSpace(jokeCreateDTO.Text))
            {
                return BadRequest("Joke content cannot be empty.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var joke = await _jokeService.CreateJokeAsync(jokeCreateDTO, userId);

            return CreatedAtAction(nameof(GetJokeById), new { id = joke.JokeId }, joke.ToJokeDTO());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJokeAsync([FromRoute] int id, [FromBody] JokeUpdateDTO jokeUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeUpdateDTO == null)
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var joke = await _jokeService.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            var jokeDTO = await _jokeService.UpdateJokeAsync(id, jokeUpdateDTO);
            return Ok(jokeDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJoke([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var joke = await _jokeService.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            await _jokeService.DeleteJokeAsync(id);
            return NoContent();
        }
        [HttpGet]
        [Route("{jokeId:int}/is-saved")]
        [Authorize]
        public async Task<IActionResult> IsJokeSaved([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token.");

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId); // Assuming a method like this
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var isSaved = await _userSavedJokeRepo.IsJokeSavedByUserAsync(new SaveJokeDTO{JokeId = jokeId}, userId);

            return Ok(new { Jokeid = jokeId, IsSaved = isSaved });
        }
    }
}