using System.Security.Claims;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.BL.Mappers;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Anekdotify.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [Route("api/joke")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        private readonly IJokeService _jokeService;
        private readonly IUserSavedJokeService _userSavedJokeService;
        private readonly IUserViewedJokesService _userViewedJokesService;
        private readonly UserManager<User> _userManager;
        public JokeController(IJokeService jokeService, IUserSavedJokeService userSavedJokeService,
            IUserViewedJokesService userViewedJokesService, UserManager<User> userManager)
        {
            _jokeService = jokeService;
            _userSavedJokeService = userSavedJokeService;
            _userViewedJokesService = userViewedJokesService;
            _userManager = userManager;
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
        [HttpGet]
        [Route("random")]
        public async Task<IActionResult> GetRandomJokeAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var viewedJokes = await _userViewedJokesService.GetViewedJokesAsync(userId);
            
            var joke = await _jokeService.GetRandomJokeAsync(viewedJokes.Value ?? []);
            if (joke == null)
            {
                return NotFound("No jokes found.");
            }
            await _userViewedJokesService.AddViewedJokeAsync(userId, joke.JokeId);
            
            user.LastJokeRetrievalDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

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

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var isSaved = await _userSavedJokeService.IsJokeSavedByUserAsync(new SaveJokeDTO{JokeId = jokeId}, userId);

            return Ok(new { Jokeid = jokeId, IsSaved = isSaved });
        }
    }
}