using System.Security.Claims;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/comment/rate")]
    [Authorize]
    public class CommentRatingController : ControllerBase
    {
        private readonly ICommentRatingService _commentRatingService;
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentRatingController> _logger;

        public CommentRatingController(ICommentRatingService commentRatingService, ICommentService commentService, ILogger<CommentRatingController> logger)
        {
            _commentRatingService = commentRatingService;
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet]
        [Route("{commentId:int}")]
        public async Task<IActionResult> GetCommentRateForUserAsync([FromRoute] int commentId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetCommentRateForUserAsync for comment {CommentId}", commentId);
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in GetCommentRateForUserAsync.");
                return Unauthorized();
            }

            var commentExists = await _commentService.CommentExistsAsync(commentId);
            if (!commentExists)
            {
                _logger.LogWarning("Comment not found for id {CommentId} in GetCommentRateForUserAsync", commentId);
                return NotFound($"Comment not found");
            }
            var userRatingOperationResult = await _commentRatingService.GetCommentRatingByUserAsync(commentId, userId);
            _logger.LogInformation("Fetched comment rating for comment {CommentId} by user {UserId}", commentId, userId);
            return Ok(userRatingOperationResult.Value);
        }

        [HttpPut]
        [Route("{commentId:int}")]
        public async Task<IActionResult> SetCommentRatingAsync([FromRoute] int commentId, [FromBody] bool? isLike)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in SetCommentRatingAsync for comment {CommentId}", commentId);
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in SetCommentRatingAsync.");
                return Unauthorized();
            }

            var commentExists = await _commentService.CommentExistsAsync(commentId);
            if (!commentExists)
            {
                _logger.LogWarning("Comment not found for id {CommentId} in SetCommentRatingAsync", commentId);
                return NotFound($"Comment not found");
            }
            var commentRatingDto = new CommentRatingDTO
            {
                CommentId = commentId,
                IsLike = isLike
            };

            var setOperationResult = await _commentRatingService.SetCommentRatingAsync(commentRatingDto, userId);
            if (setOperationResult.IsSuccess)
            {
                _logger.LogInformation("Set comment rating for comment {CommentId} by user {UserId} (IsLike: {IsLike})", commentId, userId, isLike);
                return Ok(new RatingDto { IsLike = isLike });
            }
            if (setOperationResult.IsNotFound)
            {
                _logger.LogWarning("Attempted to set rating for non-existent comment {CommentId} by user {UserId}", commentId, userId);
                return NotFound(setOperationResult.ErrorMessage);
            }
            _logger.LogWarning("Failed to set comment rating for comment {CommentId} by user {UserId}", commentId, userId);
            return Ok(new RatingDto { IsLike = isLike });
        }

        [HttpDelete]
        [Route("{commentId:int}")]
        public async Task<IActionResult> RemoveCommentRatingAsync([FromRoute] int commentId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in RemoveCommentRatingAsync for comment {CommentId}", commentId);
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims in RemoveCommentRatingAsync.");
                return Unauthorized();
            }

            var commentExists = await _commentService.CommentExistsAsync(commentId);
            if (!commentExists)
            {
                _logger.LogWarning("Comment not found for id {CommentId} in RemoveCommentRatingAsync", commentId);
                return NotFound($"Comment not found");
            }
            var removeOperationResult = await _commentRatingService.RemoveCommentRatingAsync(commentId, userId);
            if (removeOperationResult.IsNotFound)
            {
                _logger.LogWarning("Attempted to remove rating for non-existent comment {CommentId} by user {UserId}", commentId, userId);
                return NotFound(removeOperationResult.ErrorMessage);
            }
            _logger.LogInformation("Removed comment rating for comment {CommentId} by user {UserId}", commentId, userId);
            return Ok();
        }
    }
}