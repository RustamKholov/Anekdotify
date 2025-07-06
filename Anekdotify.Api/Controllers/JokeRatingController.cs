using System.Security.Claims;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/joke")]
    [Authorize]
    public class JokeRatingController : ControllerBase
    {
        private readonly IJokeRatingsService _jokeRatingsService;
        private readonly IJokeService _jokeService;
        public JokeRatingController(IJokeRatingsService jokeRatingsService, IJokeService jokeRepo)
        {
            _jokeRatingsService = jokeRatingsService;
            _jokeService = jokeRepo;
        }

        [HttpGet]
        [Route("{jokeId:int}/rating")]
        public async Task<IActionResult> GetJokeRateForUserAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId); // Assuming JokeExistsAsync in IJokeRepository
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var userRatingOperationResult = await _jokeRatingsService.GetJokeRatingByUserAsync(jokeId, userId);

            return Ok(userRatingOperationResult.Value);
        }
        [HttpPut]
        [Route("{jokeId:int}/rating")]
        public async Task<IActionResult> SetJokeRatingAsync([FromRoute] int jokeId, [FromBody] bool? isLike)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token claims.");

            var existingJoke = await _jokeService.JokeExistsAsync(jokeId);
            if (!existingJoke)
            {
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
                return Ok(new RatingDto { IsLike = isLike });
            }
            if (setOperationResult.IsNotFound)
            {
                return NotFound(setOperationResult.ErrorMessage);
            }
            return Ok(new RatingDto { IsLike = isLike });
        }
        [HttpDelete]
        [Route("{jokeId:int}/rating/delete")]
        public async Task<IActionResult> RemoveJokeRatingAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token claims.");

            var jokeExists = await _jokeService.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }
            var removeOperationResult = await _jokeRatingsService.RemoveJokeRatingAsync(jokeId, userId);

            if (removeOperationResult.IsNotFound)
            {
                return NotFound(removeOperationResult.ErrorMessage);
            }
            return Ok();
        }
    }
}