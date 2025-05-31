using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.DTOs.Jokes;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/joke")]
    [ApiController]
    public class JokeController : ControllerBase
    {
        private readonly IJokeRepository _jokeRepo;
        // Constructor to inject the ApplicationDBContext
        public JokeController(IJokeRepository jokeRepo)
        {
            _jokeRepo = jokeRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetJokesAsync([FromQuery] JokesQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var jokes = await _jokeRepo.GetAllJokesAsync(query);
            var jokesDto = jokes.Select(j => j.ToJokeDTO()).ToList();
            if (jokes == null)
            {
                return NotFound("No jokes found.");
            }
            return Ok(jokesDto);
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
        public async Task<IActionResult> CreateJokeAsync([FromBody] JokeCreateDTO jokeCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeCreateDTO == null || string.IsNullOrWhiteSpace(jokeCreateDTO.Content))
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var joke = await _jokeRepo.CreateJokeAsync(jokeCreateDTO);

            return CreatedAtAction(nameof(GetJokeById), new { id = joke.Id }, joke);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJokeAsynv([FromRoute] int id, [FromBody] JokeUpdateDTO jokeUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (jokeUpdateDTO == null || string.IsNullOrWhiteSpace(jokeUpdateDTO.Content))
            {
                return BadRequest("Joke content cannot be empty.");
            }
            var joke = await _jokeRepo.GetJokeByIdAsync(id);
            if (joke == null)
            {
                return NotFound($"Joke with ID {id} not found.");
            }
            joke = await _jokeRepo.UpdateJokeAsync(id, jokeUpdateDTO);
            return Ok(joke);
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

    }
}