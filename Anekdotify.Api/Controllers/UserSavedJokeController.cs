using System.Security.Claims;
using Anekdotify.BL.Interfaces;
using Anekdotify.Models.Models.DTOs.SaveJoke;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
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
        [Route("{jokeId:int}")]
        public async Task<IActionResult> SaveJokeAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var saveJokeDTO = new SaveJokeDTO
            {
                JokeId = jokeId
            };
            var result = await _userSavedJokeRepo.SaveJokeAsync(saveJokeDTO, userId);

            if (result.IsSuccess) return Ok(new {Saved = true, saveJokeDTO.JokeId});

            if (result.IsAlreadyExists) return Conflict(result.ErrorMessage);

            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete]
        [Route("{jokeId:int}")]
        public async Task<IActionResult> DeleteSavedJokeAsync([FromRoute] int jokeId)
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
            var saveJokeDTO = new SaveJokeDTO
            {
                JokeId = jokeId
            };
            var result = await _userSavedJokeRepo.RemoveSavedJokeAsync(saveJokeDTO, userId);

            if (result.IsNotFound)
            {
                return NotFound(result.ErrorMessage);
            }
            if (result.IsSuccess)
            {
                return Ok(new { Deleted = true, saveJokeDTO.JokeId });
            }
            return BadRequest(result.ErrorMessage);
        }
    }

}