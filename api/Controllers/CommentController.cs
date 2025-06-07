using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.DTOs.Comments;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> GetAllComments([FromQuery] CommentsQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var commentsDTO = await _commentRepo.GetAllCommentsAsync(query);
            return Ok(commentsDTO);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _commentRepo.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound($"Commnet with id {id} not found");
            }
            return Ok(comment.ToCommentDTO());
        }

        [HttpPost]
        [Route("{jokeId:int}")]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromRoute] int jokeId, [FromBody] CommentCreateDTO commentCreateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            if (!await _jokeRepo.JokeExists(jokeId))
            {
                return BadRequest($"Joke with id {jokeId} not exist");
            }
            if (commentCreateDTO == null || string.IsNullOrWhiteSpace(commentCreateDTO.CommentText))
            {
                return BadRequest("Comment content cannot be empty");
            }
            ArgumentNullException.ThrowIfNull(userId);
            var comment = commentCreateDTO.ToCommentFromCreateDTO(jokeId, userId); //TODO
            await _commentRepo.CreateCommentAsync(comment);

            return CreatedAtAction(nameof(GetCommentById), new { id = comment.CommentId }, comment.ToCommentDTO());
        }

        [HttpPut]
        [Route("{id:int}")]

        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] CommentUpdateDTO commentUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _commentRepo.UpdateCommentAsync(id, commentUpdateDTO);
            if (comment == null)
            {
                return NotFound($"Comment with id {id} not found");
            }
            return Ok(comment.ToCommentDTO());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _commentRepo.DeleteCommentAsync(id);
            if (comment == null)
            {
                return NotFound($"Commnet not found");
            }
            return Ok(comment);
        }
    }
}