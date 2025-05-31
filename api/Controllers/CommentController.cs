using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Comments;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IJokeRepository _jokeRepo;

        public CommentController(ICommentRepository commentRepo, IJokeRepository jokeRepo)
        {
            _commentRepo = commentRepo;
            _jokeRepo = jokeRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCommnets()
        {
            var comments = await _commentRepo.GetAllCommentsAsync();
            var commentsDTO = comments.Select(c => c.ToCommentDTO());
            return Ok(commentsDTO);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int id)
        {
            var comment = await _commentRepo.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound($"Commnet with id {id} not found");
            }
            return Ok(comment.ToCommentDTO());
        }
        [HttpGet("byJoke/{jokeId}")]
        public async Task<IActionResult> GetCommentsByJokeId([FromRoute] int jokeId)
        {
            var comments = await _commentRepo.GetCommentsByJokeIdAsync(jokeId);
            if (comments == null)
            {
                return NotFound("Comments not found");
            }
            return Ok(comments.Select(c => c.ToCommentDTO()));
        }

        [HttpPost]
        [Route("{jokeId}")]
        public async Task<IActionResult> CreateComment([FromRoute] int jokeId, [FromBody] CommentCreateDTO commentCreateDTO)
        {
            if (!await _jokeRepo.JokeExists(jokeId))
            {
                return BadRequest($"Joke with id {jokeId} not exist");
            }
            if (commentCreateDTO == null || string.IsNullOrWhiteSpace(commentCreateDTO.Content))
            {
                return BadRequest("Comment content cannot be empty");
            }

            var comment = commentCreateDTO.ToCommentFromCreateDTO(jokeId);
            await _commentRepo.CreateCommentAsync(comment);

            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment.ToCommentDTO());
        }

        [HttpPut]
        [Route("{id}")]

        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] CommentUpdateDTO commentUpdateDTO)
        {
            var comment = await _commentRepo.UpdateCommentAsync(id, commentUpdateDTO.ToCommentFromUpdateDTO());
            if (comment == null)
            {
                return NotFound($"Comment with id {id} not found");
            }
            return Ok(comment.ToCommentDTO());
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            var comment = await _commentRepo.DeleteCommentAsync(id);
            if (comment == null)
            {
                return NotFound($"Commnet not found");
            }
            return Ok(comment);
        }
    }
}