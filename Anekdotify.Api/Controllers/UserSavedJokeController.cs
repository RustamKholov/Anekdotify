using System.Security.Claims;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.SaveJoke;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/saved-jokes")]
    [Authorize]
    public class UserSavedJokeController : ControllerBase
    {
        private readonly IUserSavedJokeService _userSavedJokeService;

        public UserSavedJokeController(IUserSavedJokeService userSavedJokeService)
        {
            _userSavedJokeService = userSavedJokeService;
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
            var savedJokes = await _userSavedJokeService.GetSavedJokesForUserAsync(userId);
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
            var saveJokeDto = new SaveJokeDto
            {
                JokeId = jokeId
            };
            var result = await _userSavedJokeService.SaveJokeAsync(saveJokeDto, userId);

            if (result.IsSuccess) return Ok(true);

            if (result.IsAlreadyExists) return Conflict(result.ErrorMessage);

            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete]
        [Route("{jokeId:int}")]
        public async Task<IActionResult> RemoveSavedJokeAsync([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var saveJokeDto = new SaveJokeDto
            {
                JokeId = jokeId
            };
            var result = await _userSavedJokeService.RemoveSavedJokeAsync(saveJokeDto, userId);

            if (result.IsNotFound)
            {
                return NotFound(result.ErrorMessage);
            }
            if (result.IsSuccess)
            {
                return Ok(new { Deleted = true, saveJokeDto.JokeId });
            }
            return BadRequest(result.ErrorMessage);
        }
    }

}