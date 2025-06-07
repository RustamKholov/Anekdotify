using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.DTOs.Jokes;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/joke")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        private readonly IJokeRepository _jokeRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly UserManager<User> _userManager;
        private readonly IUserSavedJokeRepository _userSavedJokeRepo;
        public JokeController(IJokeRepository jokeRepo, ICommentRepository commentRepo, UserManager<User> userManager, IUserSavedJokeRepository userSavedJokeRepo)
        {
            _jokeRepo = jokeRepo;
            _commentRepo = commentRepo;
            _userManager = userManager;
            _userSavedJokeRepo = userSavedJokeRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetJokesAsync([FromQuery] JokesQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var jokeDTOs = await _jokeRepo.GetAllJokesAsync(query);
            if (jokeDTOs == null)
            {
                return NotFound("No jokes found.");
            }
            return Ok(jokeDTOs);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJokeById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            return Ok(joke);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateJokeAsync([FromBody] JokeCreateDTO jokeCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeCreateDTO == null || string.IsNullOrWhiteSpace(jokeCreateDTO.Text))
            {
                return BadRequest("Joke content cannot be empty.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var joke = await _jokeRepo.CreateJokeAsync(jokeCreateDTO, userId);

            return CreatedAtAction(nameof(GetJokeById), new { id = joke.JokeId }, joke.ToJokeDTO());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJokeAsync([FromRoute] int id, [FromBody] JokeUpdateDTO jokeUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeUpdateDTO == null)
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            var jokeDTO = await _jokeRepo.UpdateJokeAsync(id, jokeUpdateDTO);
            return Ok(jokeDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJoke([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            await _jokeRepo.DeleteJokeAsync(id);
            return NoContent();
        }
        [HttpGet]
        [Route("{jokeId:int}/is-saved")]
        [Authorize]
        public async Task<IActionResult> IsJokeSaved([FromRoute] int jokeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token.");

            var jokeExists = await _jokeRepo.JokeExistsAsync(jokeId); // Assuming a method like this
            if (!jokeExists)
            {
                return NotFound($"Joke with ID {jokeId} not found.");
            }

            var isSaved = await _userSavedJokeRepo.IsJokeSavedByUserAsync(jokeId, userId);

            return Ok(new { Jokeid = jokeId, IsSaved = isSaved });
        }
    }
}