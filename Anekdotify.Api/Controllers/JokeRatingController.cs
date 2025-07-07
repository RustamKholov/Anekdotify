using System.Security.Claims;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/joke")]
    [Authorize]
    public class JokeRatingController : ControllerBase
    {
        private readonly IJokeRatingsService _jokeRatingsService;
        private readonly IJokeService _jokeService;
        private readonly ILogger<JokeRatingController> _logger;

        public JokeRatingController(IJokeRatingsService jokeRatingsService, IJokeService jokeService, ILogger<JokeRatingController> logger)
        {
            _jokeRatingsService = jokeRatingsService;
            _jokeService = jokeService;
            _logger = logger;
        }

        [HttpGet]
        [Route("{jokeId:int}/rating")]
        public async Task<IActionResult> GetJokeRateForUserAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetJokeRateForUserAsync for joke {JokeId}", jokeId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetJokeRateForUserAsync.");
                return Unauthorized();
            }

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                _logger.LogWarning("Joke not found for id {JokeId} in GetJokeRateForUserAsync", jokeId);
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var userRatingOperationResult = await _jokeRatingsService.GetJokeRatingByUserAsync(jokeId, userId);
            _logger.LogInformation("Fetched joke rating for joke {JokeId} by user {UserId}", jokeId, userId);

            return Ok(userRatingOperationResult.Value);
        }

        [HttpPut]
        [Route("{jokeId:int}/rating")]
        public async Task<IActionResult> SetJokeRatingAsync([FromRoute] int jokeId, [FromBody] bool? isLike)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in SetJokeRatingAsync for joke {JokeId}", jokeId);
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in SetJokeRatingAsync.");
                return Unauthorized("User ID not found in token claims.");
            }

            var existingJoke = await _jokeService.JokeExistsAsync(jokeId);
            if (!existingJoke)
            {
                _logger.LogWarning("Joke not found for id {JokeId} in SetJokeRatingAsync", jokeId);
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var jokeRatingDto = new JokeRatingDto
            {
                JokeId = jokeId,
                IsLike = isLike
            };

            var setOperationResult = await _jokeRatingsService.SetJokeRatingAsync(jokeRatingDto, userId);

            if (setOperationResult.IsSuccess)
            {
                _logger.LogInformation("Set joke rating for joke {JokeId} by user {UserId} (IsLike: {IsLike})", jokeId, userId, isLike);
                return Ok(new RatingDto { IsLike = isLike });
            }
            if (setOperationResult.IsNotFound)
            {
                _logger.LogWarning("Attempted to set rating for non-existent joke {JokeId} by user {UserId}", jokeId, userId);
                return NotFound(setOperationResult.ErrorMessage);
            }
            _logger.LogWarning("Failed to set joke rating for joke {JokeId} by user {UserId}", jokeId, userId);
            return Ok(new RatingDto { IsLike = isLike });
        }

        [HttpDelete]
        [Route("{jokeId:int}/rating/delete")]
        public async Task<IActionResult> RemoveJokeRatingAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in RemoveJokeRatingAsync for joke {JokeId}", jokeId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in RemoveJokeRatingAsync.");
                return Unauthorized("User ID not found in token claims.");
            }

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                _logger.LogWarning("Joke not found for id {JokeId} in RemoveJokeRatingAsync", jokeId);
                return NotFound($"Joke with ID {jokeId} not found.");
            }
            var removeOperationResult = await _jokeRatingsService.RemoveJokeRatingAsync(jokeId, userId);

            if (removeOperationResult.IsNotFound)
            {
                _logger.LogWarning("Attempted to remove rating for non-existent joke {JokeId} by user {UserId}", jokeId, userId);
                return NotFound(removeOperationResult.ErrorMessage);
            }
            _logger.LogInformation("Removed joke rating for joke {JokeId} by user {UserId}", jokeId, userId);
            return Ok();
        }
    }
}