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

        [HttpPost]
        public async Task<IActionResult> SaveJoke([FromBody] SaveJokeDTO saveJokeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _userSavedJokeRepo.SaveJokeAsync(saveJokeDTO);

            if (result.IsSuccess) return Ok(saveJokeDTO);

            if (result.IsAlreadyExists) return Conflict(result.ErrorMessage);

            return BadRequest(result.ErrorMessage);
        }
    }

}