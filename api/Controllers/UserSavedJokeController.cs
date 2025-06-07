using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.DTOs.SaveJoke;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace api.Controllers
{
    [ApiController]
    [Route("api/saved-jokes")]
    [Authorize]
    public class UserSavedJokeController : ControllerBase
    {
        private readonly IUserSavedJokeRepository _userSavedJokeRepo;

        public UserSavedJokeController(IUserSavedJokeRepository userSavedJokeRepo)
        {
            _userSavedJokeRepo = userSavedJokeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetSavedJokesForUser()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return NotFound("User not found");
            }
            var savedJokes = await _userSavedJokeRepo.GetSavedJokesForUserAsync(userId);
            return Ok(savedJokes);
        }

        [HttpPost]
        public async Task<IActionResult> SaveJokeAsync([FromBody] SaveJokeDTO saveJokeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _userSavedJokeRepo.SaveJokeAsync(saveJokeDTO, userId);

            if (result.IsSuccess) return Ok(saveJokeDTO);

            if (result.IsAlreadyExists) return Conflict(result.ErrorMessage);

            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSavedJokeAsync([FromBody] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (userId == null)
            {
                return NotFound("User not found");
            }
            var result = await _userSavedJokeRepo.RemoveSavedJokeAsync(jokeId, userId);

            if (result.IsNotFound)
            {
                return NotFound(result.ErrorMessage);
            }
            if (result.IsSuccess)
            {
                return Ok(new { JokeId = jokeId });
            }
            return BadRequest(result.ErrorMessage);
        }
    }

}