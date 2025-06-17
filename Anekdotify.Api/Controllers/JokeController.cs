using System.Security.Claims;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.BL.Mappers;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [Route("api/joke")]
    [ApiController]
    [Authorize]
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

            if(user.LastJokeRetrievalDate.HasValue &&
               user.LastJokeRetrievalDate.Value.AddHours(24) > DateTime.UtcNow)
            {
                if (!User.IsInRole("Admin") || !User.IsInRole("Moderator"))    // moderators and admins do not have this restriction
                    return BadRequest("User can only get a joke once every 24 hours");
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

            if (!User.IsInRole("Admin") || !User.IsInRole("Moderator"))
            {
                await _userViewedJokesService.AddViewedJokeAsync(userId, joke.JokeId);
            }
            
            user.LastJokeRetrievalDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(joke);
        }
        [HttpGet]
        [Route("random/isActive")]
        public async Task<IActionResult> IsRandomJokeActiveAsync()
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
            if (!User.IsInRole("Admin") || !User.IsInRole("Moderator"))
            {
                if (user.LastJokeRetrievalDate.HasValue &&
                    user.LastJokeRetrievalDate.Value.AddHours(24) > DateTime.UtcNow)
                {
                    var jokeAvailableAt = user.LastJokeRetrievalDate.Value.AddHours(24);
                    return Ok(new IsActiveRandomResponse { IsActive = false, NextRandomJokeAvailableAt = jokeAvailableAt });
                }
            }
            return Ok(new IsActiveRandomResponse { IsActive = true, NextRandomJokeAvailableAt = DateTime.UtcNow});
        }
        [HttpGet]
        [Route("last-viewed")]
        public async Task<IActionResult> GetLastViewedJokeAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var lastViewedJokeResult = await _userViewedJokesService.GetLastViewedJokeAsync(userId);
            if (!lastViewedJokeResult.IsSuccess)
            {
                return NotFound(lastViewedJokeResult.ErrorMessage);
            }
            if (lastViewedJokeResult.Value == null)
            {
                return NotFound("No viewed jokes found for this user.");
            }
            return Ok(lastViewedJokeResult.Value);
        }
        [HttpGet]
        [Route("last-viewed/isActual")]
        public async Task<IActionResult> GetIsLastViewedActual()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var isActualRes = await _userViewedJokesService.IsLastViewedJokeActualAsync(userId);
            if (!isActualRes.IsSuccess)
            {
                return NotFound(isActualRes.ErrorMessage);
            }
            return Ok(isActualRes.Value);
        }


        [HttpPost]
        [Authorize(Roles ="Admin, Moderator")]
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
        [Authorize(Roles = "Admin, Moderator")]
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
        [Authorize(Roles = "Admin, Moderator")]
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

            return Ok(isSaved);
        }
    }
}