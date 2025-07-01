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

            var jokeDtOs = await _jokeService.GetAllJokesAsync(query);
            return Ok(jokeDtOs);
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

            if (user.LastJokeRetrievalDate.HasValue &&
                user.LastJokeRetrievalDate.Value.AddHours(24) > DateTime.UtcNow)
            {
                if (!User.IsInRole("Admin") ||
                    !User.IsInRole("Moderator")) // moderators and admins do not have this restriction
                    return BadRequest("User can only get a joke once every 24 hours");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }

            var viewedJokes = await _userViewedJokesService.GetViewedJokesAsync(userId);

            var joke = await _jokeService.GetRandomJokeAsync(viewedJokes.Value ?? []);

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
                    return Ok(new IsActiveRandomResponse
                        { IsActive = false, NextRandomJokeAvailableAt = jokeAvailableAt });
                }
            }

            return Ok(new IsActiveRandomResponse { IsActive = true, NextRandomJokeAvailableAt = DateTime.UtcNow });
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
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> CreateJokeAsync([FromBody] JokeCreateDto? jokeCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (jokeCreateDto == null || string.IsNullOrWhiteSpace(jokeCreateDto.Text))
            {
                return BadRequest("Joke content cannot be empty.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }

            var joke = await _jokeService.CreateJokeAsync(jokeCreateDto, userId);

            return CreatedAtAction(nameof(GetJokeById), new { id = joke.JokeId }, joke.ToJokeDto());
        }

        [HttpPost]
        [Route("suggest")]
        public async Task<IActionResult> SuggestJokeAsync([FromBody] JokeCreateDto? jokeCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (jokeCreateDto == null || string.IsNullOrWhiteSpace(jokeCreateDto.Text))
            {
                return BadRequest("Joke content cannot be empty.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }

            jokeCreateDto.SourceId = -4; // Set SourceId to -1 for suggested jokes
            var joke = await _jokeService.SuggestJokeAsync(jokeCreateDto, userId);
            return Ok(joke);
        }

        [HttpGet]
        [Route("suggested-by-me")]
        public async Task<IActionResult> GetSuggestedByMeJokes()
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

            var suggestedBm = await _jokeService.GetSuggestedByMeJokes(userId);
            return Ok(suggestedBm);
        }

        [HttpGet]
        [Route("{jokeId:int}/created-by-me")]
        public async Task<IActionResult> GetCreatedByMeJoke([FromRoute] int jokeId)
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
            var createdByMeRes = await _jokeService.IsJokeOwnerAsync(jokeId, userId);
            return Ok(createdByMeRes);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJokeAsync([FromRoute] int id, [FromBody] JokeUpdateDto? jokeUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeUpdateDto == null)
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var jokeDto = await _jokeService.GetJokeByIdAsync(id);
            if (jokeDto == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }

            Joke joke;
            if (User.IsInRole("User"))
            {
                joke = await _jokeService.UpdateJokeByUserAsync(id, jokeUpdateDto);
            }
            else
            {
                joke = await _jokeService.UpdateJokeAsync(id, jokeUpdateDto);
            }
            return Ok(joke);
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

            var isSaved = await _userSavedJokeService.IsJokeSavedByUserAsync(new SaveJokeDto { JokeId = jokeId }, userId);

            return Ok(isSaved);
        }
        
    }
}