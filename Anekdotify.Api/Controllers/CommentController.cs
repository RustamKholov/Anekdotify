using System.Security.Claims;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.BL.Mappers;
using Anekdotify.Models.DTOs.Comments;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IJokeService _jokeService;

        public CommentController(ICommentService commentService, IJokeService jokeService)
        {
            _commentService = commentService;
            _jokeService = jokeService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllComments([FromQuery] CommentsQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var commentsDTO = await _commentService.GetAllCommentsAsync(query);
            return Ok(commentsDTO);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound($"Commnet with id {id} not found");
            }
            return Ok(comment.ToCommentDTO());
        }

        [HttpPost]
        [Route("{jokeId:int}")]
        
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
            if (!await _jokeService.JokeExistsAsync(jokeId))
            {
                return BadRequest($"Joke with id {jokeId} not exist");
            }
            if (commentCreateDTO == null || string.IsNullOrWhiteSpace(commentCreateDTO.CommentText))
            {
                return BadRequest("Comment content cannot be empty");
            }
            if (commentCreateDTO.ParentCommentId != null)
            {
                var parentComment = await _commentService.GetCommentByIdAsync(commentCreateDTO.ParentCommentId.Value);
                if (parentComment == null)
                {
                    return BadRequest($"Parent comment with id {commentCreateDTO.ParentCommentId} not found");
                }
                if (parentComment.JokeId != jokeId)
                {
                    return BadRequest("Parent comment does not belong to the same joke");
                }
            }
            ArgumentNullException.ThrowIfNull(userId);
            var comment = commentCreateDTO.ToCommentFromCreateDTO(jokeId, userId);
            await _commentService.CreateCommentAsync(comment);

            return Created();
        }

        [HttpPut]
        [Route("{id:int}")]

        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] CommentUpdateDTO commentUpdateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _commentService.UpdateCommentAsync(id, commentUpdateDTO);
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
            var comment = await _commentService.DeleteCommentAsync(id);
            if (comment == null)
            {
                return NotFound($"Commnet not found");
            }
            return Ok(comment);
        }
    }
}