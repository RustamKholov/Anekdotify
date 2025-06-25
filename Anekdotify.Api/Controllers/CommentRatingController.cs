using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/comment/rate")]
    [Authorize]
    public class CommentRatingController : ControllerBase
    {
        private readonly ICommentRatingService _commentRatingService;
        private readonly ICommentService _commentService;
        public CommentRatingController(ICommentRatingService commentRatingService, ICommentService commentService)
        {
            _commentRatingService = commentRatingService;
            _commentService = commentService;
        }

        [HttpGet]
        [Route("{commentId:int}")]
        public async Task<IActionResult> GetCommentRateForUserAsync([FromRoute] int commentId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var commentExists = await _commentService.CommentExistsAsync(commentId);
            if (!commentExists)
            {
                return NotFound($"Comment not found");
            }
            var userRatingOperationResult = await _commentRatingService.GetCommentRatingByUserAsync(commentId, userId);
            return Ok(userRatingOperationResult.Value);
        }

        [HttpPut]
        [Route("{commentId:int}")]
        public async Task<IActionResult> SetCommentRatingAsync([FromRoute] int commentId, [FromBody] bool? isLike)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (isLike == null)
            {
                return BadRequest("Rating value (IsLike) is required.");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var commentExists = await _commentService.CommentExistsAsync(commentId);
            if (!commentExists)
            {
                return NotFound($"Comment not found");
            }
            var commentRatingDTO = new CommentRatingDTO
            {
                CommentId = commentId,
                IsLike = isLike
            };

            var setOperationResult = await _commentRatingService.SetCommentRatingAsync(commentRatingDTO, userId);
            if (setOperationResult.IsSuccess)
            {
                return Ok(new RatingDTO { IsLike = isLike });
            }
            if (setOperationResult.IsNotFound)
            {
                return NotFound(setOperationResult.ErrorMessage);
            }
            return Ok(new RatingDTO { IsLike = isLike });
        }

        [HttpDelete]
        [Route("{commentId:int}")]

        public async Task<IActionResult> RemoveCommentRatingAsync([FromRoute] int commentId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var commentExists = await _commentService.CommentExistsAsync(commentId);
            if (!commentExists)
            {
                return NotFound($"Comment not found");
            }
            var removeOperationResult = await _commentRatingService.RemoveCommentRatingAsync(commentId, userId);
            if (removeOperationResult.IsNotFound)
            {
                return NotFound(removeOperationResult.ErrorMessage);
            }
            return Ok();
        }
    }
}