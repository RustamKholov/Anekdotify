using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using api.DTOs.JokeRating;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]

    [Authorize]
    public class JokeRatingController : ControllerBase
    {
        private readonly IJokeRatingsRepository _jokeRatingsRepo;
        private readonly IJokeRepository _jokeRepo;
        public JokeRatingController(IJokeRatingsRepository jokeRatingsRepo, IJokeRepository jokeRepo)
        {
            _jokeRatingsRepo = jokeRatingsRepo;
            _jokeRepo = jokeRepo;
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

            var jokeExists = await _jokeRepo.JokeExistsAsync(jokeId); // Assuming JokeExistsAsync in IJokeRepository
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var userRatingOperationResult = await _jokeRatingsRepo.GetJokeRatingByUserAsync(jokeId, userId);

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
            if (!isLike.HasValue)
            {
                return BadRequest("Rating value (IsLike) is required.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token claims.");

            var jokeExists = await _jokeRepo.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var jokeRatingDTO = new JokeRatingDTO
            {
                JokeId = jokeId,
                IsLike = isLike
            };
            var setOperationResult = await _jokeRatingsRepo.SetJokeRatingAsync(jokeRatingDTO, userId);
            if (setOperationResult.IsSuccess)
            {
                return Ok(setOperationResult.Value);
            }
            if (setOperationResult.IsNotFound)
            {
                return NotFound(setOperationResult.ErrorMessage);
            }
            return Ok();
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

            var jokeExists = await _jokeRepo.JokeExistsAsync(jokeId);
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }
            var removeOperationResult = await _jokeRatingsRepo.RemoveJokeRatingAsync(jokeId, userId);

            if (removeOperationResult.IsNotFound)
            {
                return NotFound(removeOperationResult.ErrorMessage);
            }
            return Ok();
        }
    }
}