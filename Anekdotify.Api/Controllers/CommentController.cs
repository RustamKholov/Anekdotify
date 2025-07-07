using System.Security.Claims;
using Anekdotify.BL.Helpers;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.BL.Mappers;
using Anekdotify.Models.DTOs.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IJokeService _jokeService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, IJokeService jokeService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _jokeService = jokeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllComments([FromQuery] CommentsQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetAllComments.");
                return BadRequest(ModelState);
            }
            var commentsDto = await _commentService.GetAllCommentsAsync(query);
            _logger.LogInformation("Fetched all comments (Count: {Count})", commentsDto.Count);
            return Ok(commentsDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCommentById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetCommentById for id {Id}", id);
                return BadRequest(ModelState);
            }
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Comment not found for id {Id}", id);
                return NotFound($"Commnet with id {id} not found");
            }
            _logger.LogInformation("Fetched comment by id {Id}", id);
            return Ok(comment);
        }

        [HttpGet]
        [Route("{commentId:int}/created-by-me")]
        public async Task<IActionResult> GetCommentsCreatedByMe([FromRoute] int commentId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetCommentsCreatedByMe for commentId {CommentId}", commentId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetCommentsCreatedByMe.");
                return Unauthorized("User ID not found in token claims.");
            }
            var res = await _commentService.IsCommentOwnerAsync(commentId, userId);
            _logger.LogInformation("Checked comment ownership for comment {CommentId} by user {UserId}: {IsOwner}", commentId, userId, res);
            return Ok(res);
        }

        [HttpPost]
        [Route("{jokeId:int}")]
        public async Task<IActionResult> CreateComment([FromRoute] int jokeId, [FromBody] CommentCreateDto? commentCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in CreateComment for jokeId {JokeId}", jokeId);
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in CreateComment.");
                return Unauthorized("User ID not found in token claims.");
            }
            if (!await _jokeService.JokeExistsAsync(jokeId))
            {
                _logger.LogWarning("Attempted to create comment for non-existent joke {JokeId}", jokeId);
                return BadRequest($"Joke with id {jokeId} not exist");
            }
            if (commentCreateDto == null || string.IsNullOrWhiteSpace(commentCreateDto.CommentText))
            {
                _logger.LogWarning("Attempted to create comment with empty content for joke {JokeId} by user {UserId}", jokeId, userId);
                return BadRequest("Comment content cannot be empty");
            }
            if (commentCreateDto.ParentCommentId != null && commentCreateDto.ParentCommentId != 0)
            {
                var parentComment = await _commentService.GetCommentByIdAsync(commentCreateDto.ParentCommentId.Value);
                if (parentComment == null)
                {
                    _logger.LogWarning("Parent comment not found for id {ParentCommentId} in CreateComment", commentCreateDto.ParentCommentId);
                    return BadRequest($"Parent comment with id {commentCreateDto.ParentCommentId} not found");
                }
                if (parentComment.JokeId != jokeId)
                {
                    _logger.LogWarning("Parent comment {ParentCommentId} does not belong to joke {JokeId}", commentCreateDto.ParentCommentId, jokeId);
                    return BadRequest("Parent comment does not belong to the same joke");
                }
            }
            var comment = commentCreateDto.ToCommentFromCreateDto(jokeId, userId);
            var createdComment = await _commentService.CreateCommentAsync(comment);
            _logger.LogInformation("Created comment {CommentId} for joke {JokeId} by user {UserId}", createdComment.CommentId, jokeId, userId);

            return Created(nameof(_commentService.CreateCommentAsync), createdComment);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] CommentUpdateDto commentUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in UpdateComment for id {Id}", id);
                return BadRequest(ModelState);
            }
            var comment = await _commentService.UpdateCommentAsync(id, commentUpdateDto);
            if (comment == null)
            {
                _logger.LogWarning("Attempted to update non-existent comment {Id}", id);
                return NotFound($"Comment with id {id} not found");
            }
            _logger.LogInformation("Updated comment {CommentId}", id);
            return Ok(comment.ToCommentDto());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in DeleteComment for id {Id}", id);
                return BadRequest(ModelState);
            }
            var comment = await _commentService.DeleteCommentAsync(id);
            if (comment == null)
            {
                _logger.LogWarning("Attempted to delete non-existent comment {Id}", id);
                return NotFound($"Commnet not found");
            }
            _logger.LogInformation("Deleted comment {CommentId}", id);
            return Ok(comment);
        }

        [HttpGet]
        [Route("all-my-comments")]
        public async Task<IActionResult> GetAllMyComments([FromQuery] CommentsQueryObject query)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetAllMyComments.");
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetAllMyComments.");
                return Unauthorized("User ID not found in token claims.");
            }

            if (query.UserId == null || query.UserId != userId)
            {
                _logger.LogWarning("Incorrect user ID in GetAllMyComments. QueryUserId: {QueryUserId}, ActualUserId: {UserId}", query.UserId, userId);
                return BadRequest("Incorrect user ID");
            }
            var commentsDto = await _commentService.GetAllCommentsAsync(query);
            _logger.LogInformation("Fetched all my comments for user {UserId} (Count: {Count})", userId, commentsDto.Count);
            return Ok(commentsDto);
        }
    }
}