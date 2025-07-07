using System.Security.Claims;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.SaveJoke;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/saved-jokes")]
    [Authorize]
    public class UserSavedJokeController : ControllerBase
    {
        private readonly IUserSavedJokeService _userSavedJokeService;
        private readonly ILogger<UserSavedJokeController> _logger;

        public UserSavedJokeController(IUserSavedJokeService userSavedJokeService, ILogger<UserSavedJokeController> logger)
        {
            _userSavedJokeService = userSavedJokeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSavedJokesForUser()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetSavedJokesForUser.");
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("User ID not found in token claims in GetSavedJokesForUser.");
                return NotFound("User not found");
            }
            var savedJokes = await _userSavedJokeService.GetSavedJokesForUserAsync(userId);
            _logger.LogInformation("Fetched saved jokes for user {UserId} (Count: {Count})", userId, savedJokes.Count);
            return Ok(savedJokes);
        }

        [HttpPost]
        [Route("{jokeId:int}")]
        public async Task<IActionResult> SaveJokeAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in SaveJokeAsync for jokeId {JokeId}", jokeId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in SaveJokeAsync.");
                return Unauthorized();
            }
            var saveJokeDto = new SaveJokeDto
            {
                JokeId = jokeId
            };
            var result = await _userSavedJokeService.SaveJokeAsync(saveJokeDto, userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Saved joke {JokeId} for user {UserId}", jokeId, userId);
                return Ok(true);
            }

            if (result.IsAlreadyExists)
            {
                _logger.LogWarning("Attempted to save already saved joke {JokeId} for user {UserId}", jokeId, userId);
                return Conflict(result.ErrorMessage);
            }

            _logger.LogWarning("Failed to save joke {JokeId} for user {UserId}: {Error}", jokeId, userId, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete]
        [Route("{jokeId:int}")]
        public async Task<IActionResult> RemoveSavedJokeAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in RemoveSavedJokeAsync for jokeId {JokeId}", jokeId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in RemoveSavedJokeAsync.");
                return Unauthorized();
            }

            var saveJokeDto = new SaveJokeDto
            {
                JokeId = jokeId
            };
            var result = await _userSavedJokeService.RemoveSavedJokeAsync(saveJokeDto, userId);

            if (result.IsNotFound)
            {
                _logger.LogWarning("Attempted to remove non-existent saved joke {JokeId} for user {UserId}", jokeId, userId);
                return NotFound(result.ErrorMessage);
            }
            if (result.IsSuccess)
            {
                _logger.LogInformation("Removed saved joke {JokeId} for user {UserId}", jokeId, userId);
                return Ok(new { Deleted = true, saveJokeDto.JokeId });
            }
            _logger.LogWarning("Failed to remove saved joke {JokeId} for user {UserId}: {Error}", jokeId, userId, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }
    }

}