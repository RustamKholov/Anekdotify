using System.Security.Claims;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.BL.Mappers;
using Anekdotify.Models.DTOs.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
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
            var commentsDto = await _commentService.GetAllCommentsAsync(query);
            return Ok(commentsDto);
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
            return Ok(comment.ToCommentDto());
        }

        [HttpGet]
        [Route("{commentId:int}/created-by-me")]
        public async Task<IActionResult> GetCommentsCreatedByMe([FromRoute] int commentId)
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
            var res = await _commentService.IsCommentOwnerAsync(commentId, userId);
            return Ok(res);
        }

        [HttpPost]
        [Route("{jokeId:int}")]

        public async Task<IActionResult> CreateComment([FromRoute] int jokeId, [FromBody] CommentCreateDto? commentCreateDto)
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
            if (commentCreateDto == null || string.IsNullOrWhiteSpace(commentCreateDto.CommentText))
            {
                return BadRequest("Comment content cannot be empty");
            }
            if (commentCreateDto.ParentCommentId != null && commentCreateDto.ParentCommentId != 0)
            {
                var parentComment = await _commentService.GetCommentByIdAsync(commentCreateDto.ParentCommentId.Value);
                if (parentComment == null)
                {
                    return BadRequest($"Parent comment with id {commentCreateDto.ParentCommentId} not found");
                }
                if (parentComment.JokeId != jokeId)
                {
                    return BadRequest("Parent comment does not belong to the same joke");
                }
            }
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token claims.");
            }
            var comment = commentCreateDto.ToCommentFromCreateDto(jokeId, userId);
            var createdComment = await _commentService.CreateCommentAsync(comment);

            return Created(nameof(_commentService.CreateCommentAsync), createdComment);
        }

        [HttpPut]
        [Route("{id:int}")]

        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] CommentUpdateDto commentUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var comment = await _commentService.UpdateCommentAsync(id, commentUpdateDto);
            if (comment == null)
            {
                return NotFound($"Comment with id {id} not found");
            }
            return Ok(comment.ToCommentDto());
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

        [HttpGet]
        [Route("all-my-comments")]
        public async Task<IActionResult> GetAllMyComments([FromQuery] CommentsQueryObject query)
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
            
            if (query.UserId == null || query.UserId != userId)
            {
                return BadRequest("Incorrect user ID");
            }
            var commentsDto = await _commentService.GetAllCommentsAsync(query);
            return Ok(commentsDto);
        }
    }
}