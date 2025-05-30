using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTOs;
using api.Models;
using api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/joke")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        private readonly JokeRepository _jokeRepo;
        // Constructor to inject the ApplicationDBContext
        public JokeController(JokeRepository jokeRepo)
        {
            _jokeRepo = jokeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetJokesAsync()
        {
            var jokes = await _jokeRepo.GetAllJokesAsync();
            if (jokes == null)
            {
                return NotFound("No jokes found.");
            }
            return Ok(jokes);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJokeById([FromRoute] int id)
        {
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            return Ok(joke);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJokeAsync([FromBody] JokeDTO jokeDTO)
        {
            if (jokeDTO == null || string.IsNullOrWhiteSpace(jokeDTO.Content))
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var joke = await _jokeRepo.CreateJokeAsync(jokeDTO);

            return CreatedAtAction(nameof(GetJokeById), new { id = joke.Id }, joke);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJokeAsynv([FromRoute] int id, [FromBody] JokeDTO jokeDTO)
        {
            if (jokeDTO == null || string.IsNullOrWhiteSpace(jokeDTO.Content))
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            joke = await _jokeRepo.UpdateJokeAsync(id, jokeDTO);
            return Ok(joke);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJoke([FromRoute] int id)
        {
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            await _jokeRepo.DeleteJokeAsync(id);
            return NoContent();
        }

    }
}