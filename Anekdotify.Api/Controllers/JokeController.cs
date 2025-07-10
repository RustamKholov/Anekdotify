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
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<JokeController> _logger;

        public JokeController(IJokeService jokeService, IUserSavedJokeService userSavedJokeService,
            IUserViewedJokesService userViewedJokesService, UserManager<User> userManager, ILogger<JokeController> logger)
        {
            _jokeService = jokeService;
            _userSavedJokeService = userSavedJokeService;
            _userViewedJokesService = userViewedJokesService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetJokesAsync([FromQuery] JokesQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetJokesAsync.");
                return BadRequest(ModelState);
            }

            var jokeDtOs = await _jokeService.GetAllJokesAsync(query);
            _logger.LogInformation("Fetched jokes (Count: {Count})", jokeDtOs.Count);
            return Ok(jokeDtOs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJokeById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetJokeById for id {Id}", id);
                return BadRequest(ModelState);
            }

            var joke = await _jokeService.GetJokeByIdAsync(id);
            if (joke == null)
            {
                _logger.LogWarning("Joke not found for id {Id}", id);
                return NotFound($"Joke with ID {id} not found.");
            }

            _logger.LogInformation("Fetched joke by id {Id}", id);
            return Ok(joke);
        }

        [HttpGet]
        [Route("random")]
        public async Task<IActionResult> GetRandomJokeAsync([FromQuery] RandomJokeQueryObject? query = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetRandomJokeAsync.");
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found in GetRandomJokeAsync.");
                return Unauthorized("User not found.");
            }

            if (user.LastJokeRetrievalDate.HasValue &&
                user.LastJokeRetrievalDate.Value.AddHours(24) > DateTime.UtcNow)
            {
                if (!User.IsInRole("Admin") || !User.IsInRole("Moderator"))
                {
                    _logger.LogWarning("User {UserId} tried to get a joke before 24h limit.", user.Id);
                    return BadRequest("User can only get a joke once every 24 hours");
                }
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetRandomJokeAsync.");
                return Unauthorized("User ID not found in token claims.");
            }

            var viewedJokes = await _userViewedJokesService.GetViewedJokesAsync(userId);
            JokeDto joke;
            if (query != null && (query.ClassificationIds.Count != 0 || query.ClassificationIds.Count != 0))
            {
                joke = await _jokeService.GetRandomJokeAsync(viewedJokes.Value ?? [], query);
            }
            else
            {
                joke = await _jokeService.GetRandomJokeAsync(viewedJokes.Value ?? []);
            }

            if (!User.IsInRole("Admin") || !User.IsInRole("Moderator"))
            {
                await _userViewedJokesService.AddViewedJokeAsync(userId, joke.JokeId);
                _logger.LogInformation("Added viewed joke {JokeId} for user {UserId}", joke.JokeId, userId);
            }

            user.LastJokeRetrievalDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Returned random joke {JokeId} for user {UserId}", joke.JokeId, userId);
            return Ok(joke);
        }

        [HttpGet]
        [Route("random/isActive")]
        public async Task<IActionResult> IsRandomJokeActiveAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in IsRandomJokeActiveAsync.");
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not found in IsRandomJokeActiveAsync.");
                return Unauthorized("User not found.");
            }

            if (!User.IsInRole("Admin") || !User.IsInRole("Moderator"))
            {
                if (user.LastJokeRetrievalDate.HasValue &&
                    user.LastJokeRetrievalDate.Value.AddHours(24) > DateTime.UtcNow)
                {
                    var jokeAvailableAt = user.LastJokeRetrievalDate.Value.AddHours(24);
                    _logger.LogInformation("Random joke not active for user {UserId}, available at {AvailableAt}", user.Id, jokeAvailableAt);
                    return Ok(new IsActiveRandomResponse
                    { IsActive = false, NextRandomJokeAvailableAt = jokeAvailableAt });
                }
            }

            _logger.LogInformation("Random joke is active for user {UserId}", user.Id);
            return Ok(new IsActiveRandomResponse { IsActive = true, NextRandomJokeAvailableAt = DateTime.UtcNow });
        }

        [HttpGet]
        [Route("last-viewed")]
        public async Task<IActionResult> GetLastViewedJokeAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetLastViewedJokeAsync.");
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetLastViewedJokeAsync.");
                return Unauthorized("User ID not found in token claims.");
            }

            var lastViewedJokeResult = await _userViewedJokesService.GetLastViewedJokeAsync(userId);
            if (!lastViewedJokeResult.IsSuccess)
            {
                _logger.LogWarning("No last viewed joke found for user {UserId}: {Error}", userId, lastViewedJokeResult.ErrorMessage);
                return NotFound(lastViewedJokeResult.ErrorMessage);
            }

            if (lastViewedJokeResult.Value == null)
            {
                _logger.LogWarning("No viewed jokes found for user {UserId}", userId);
                return NotFound("No viewed jokes found for this user.");
            }

            _logger.LogInformation("Returned last viewed joke {JokeId} for user {UserId}", lastViewedJokeResult.Value.JokeId, userId);
            return Ok(lastViewedJokeResult.Value);
        }

        [HttpGet]
        [Route("last-viewed/isActual")]
        public async Task<IActionResult> GetIsLastViewedActual()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetIsLastViewedActual.");
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetIsLastViewedActual.");
                return Unauthorized("User ID not found in token claims.");
            }

            var isActualRes = await _userViewedJokesService.IsLastViewedJokeActualAsync(userId);
            if (!isActualRes.IsSuccess)
            {
                _logger.LogWarning("Failed to get is-last-viewed-joke-actual for user {UserId}: {Error}", userId, isActualRes.ErrorMessage);
                return NotFound(isActualRes.ErrorMessage);
            }

            _logger.LogInformation("Returned is-last-viewed-joke-actual for user {UserId}: {IsActual}", userId, isActualRes.Value);
            return Ok(isActualRes.Value);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> CreateJokeAsync([FromBody] JokeCreateDto? jokeCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in CreateJokeAsync.");
                return BadRequest(ModelState);
            }

            if (jokeCreateDto == null || string.IsNullOrWhiteSpace(jokeCreateDto.Text))
            {
                _logger.LogWarning("Attempted to create joke with empty content.");
                return BadRequest("Joke content cannot be empty.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in CreateJokeAsync.");
                return Unauthorized("User ID not found in token claims.");
            }

            var joke = await _jokeService.CreateJokeAsync(jokeCreateDto, userId);

            _logger.LogInformation("Created joke {JokeId} by user {UserId}", joke.JokeId, userId);
            return CreatedAtAction(nameof(GetJokeById), new { id = joke.JokeId }, joke.ToJokeDto());
        }

        [HttpPost]
        [Route("suggest")]
        public async Task<IActionResult> SuggestJokeAsync([FromBody] JokeCreateDto? jokeCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in SuggestJokeAsync.");
                return BadRequest(ModelState);
            }

            if (jokeCreateDto == null || string.IsNullOrWhiteSpace(jokeCreateDto.Text))
            {
                _logger.LogWarning("Attempted to suggest joke with empty content.");
                return BadRequest("Joke content cannot be empty.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in SuggestJokeAsync.");
                return Unauthorized("User ID not found in token claims.");
            }

            jokeCreateDto.SourceId = -4; // Set SourceId to -1 for suggested jokes
            var joke = await _jokeService.SuggestJokeAsync(jokeCreateDto, userId);
            _logger.LogInformation("User {UserId} suggested a joke {JokeId}", userId, joke.JokeId);
            return Ok(joke);
        }

        [HttpGet]
        [Route("suggested-by-me")]
        public async Task<IActionResult> GetSuggestedByMeJokes()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetSuggestedByMeJokes.");
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetSuggestedByMeJokes.");
                return Unauthorized("User ID not found in token claims.");
            }

            var suggestedBm = await _jokeService.GetSuggestedByMeJokes(userId);
            _logger.LogInformation("Fetched suggested jokes by user {UserId} (Count: {Count})", userId, suggestedBm.Count);
            return Ok(suggestedBm);
        }

        [HttpGet]
        [Route("{jokeId:int}/created-by-me")]
        public async Task<IActionResult> GetCreatedByMeJoke([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetCreatedByMeJoke for jokeId {JokeId}", jokeId);
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetCreatedByMeJoke.");
                return Unauthorized("User ID not found in token claims.");
            }
            var createdByMeRes = await _jokeService.IsJokeOwnerAsync(jokeId, userId);
            _logger.LogInformation("Checked joke ownership for joke {JokeId} by user {UserId}: {IsOwner}", jokeId, userId, createdByMeRes);
            return Ok(createdByMeRes);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJokeAsync([FromRoute] int id, [FromBody] JokeUpdateDto? jokeUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in UpdateJokeAsync for id {Id}", id);
                return BadRequest(ModelState);
            }
            if (jokeUpdateDto == null)
            {
                _logger.LogWarning("Attempted to update joke {Id} with empty content.", id);
                return BadRequest("Joke content cannot be empty.");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in UpdateJokeAsync.");
                return Unauthorized("User ID not found in token claims.");
            }
            var jokeDto = await _jokeService.GetJokeByIdAsync(id);
            if (jokeDto == null)
            {
                _logger.LogWarning("Attempted to update non-existent joke {Id}", id);
                return NotFound($"Joke with ID {id} not found.");
            }

            Joke joke;
            if (User.IsInRole("User"))
            {
                joke = await _jokeService.UpdateJokeByUserAsync(id, jokeUpdateDto);
                _logger.LogInformation("User {UserId} updated their joke {JokeId}", userId, id);
            }
            else
            {
                joke = await _jokeService.UpdateJokeAsync(id, jokeUpdateDto);
                _logger.LogInformation("Admin/Moderator updated joke {JokeId}", id);
            }
            return Ok(joke);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> DeleteJoke([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in DeleteJoke for id {Id}", id);
                return BadRequest(ModelState);
            }
            var joke = await _jokeService.GetJokeByIdAsync(id);
            if (joke == null)
            {
                _logger.LogWarning("Attempted to delete non-existent joke {Id}", id);
                return NotFound($"Joke with ID {id} not found.");
            }
            await _jokeService.DeleteJokeAsync(id);
            _logger.LogInformation("Deleted joke {JokeId}", id);
            return NoContent();
        }

        [HttpGet]
        [Route("{jokeId:int}/is-saved")]
        public async Task<IActionResult> IsJokeSaved([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in IsJokeSaved for jokeId {JokeId}", jokeId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in IsJokeSaved.");
                return Unauthorized("User ID not found in token.");
            }

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                _logger.LogWarning("Attempted to check is-saved for non-existent joke {JokeId}", jokeId);
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var isSaved = await _userSavedJokeService.IsJokeSavedByUserAsync(new SaveJokeDto { JokeId = jokeId }, userId);

            _logger.LogInformation("Checked if joke {JokeId} is saved by user {UserId}: {IsSaved}", jokeId, userId, isSaved);
            return Ok(isSaved);
        }
    }
}