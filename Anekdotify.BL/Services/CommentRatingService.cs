using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services
{
    public class CommentRatingService : ICommentRatingService
    {
        private readonly ICommentRatingRepository _commentRatingRepository;
        private readonly IDistributedCache _cacheService;
        private readonly ILogger<CommentRatingService> _logger;

        public CommentRatingService(
            ICommentRatingRepository commentRatingRepository,
            IDistributedCache cacheService,
            ILogger<CommentRatingService> logger)
        {
            _commentRatingRepository = commentRatingRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<OperationResult<RatingDto>> GetCommentRatingByUserAsync(int commentId, string userId)
        {
            try
            {
                var result = await _commentRatingRepository.GetCommentRatingByUserAsync(commentId, userId);
                _logger.LogInformation("Fetched comment rating for comment {CommentId} by user {UserId}: {Found}", commentId, userId, result.Value != null);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comment rating for comment {CommentId} by user {UserId}", commentId, userId);
                throw;
            }
        }

        public async Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId)
        {
            try
            {
                await _cacheService.RemoveAsync($"comment_{commentId}");
                var result = await _commentRatingRepository.RemoveCommentRatingAsync(commentId, userId);
                _logger.LogInformation("Removed comment rating for comment {CommentId} by user {UserId} (Success: {Success})", commentId, userId, result.IsSuccess);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing comment rating for comment {CommentId} by user {UserId}", commentId, userId);
                throw;
            }
        }

        public async Task<OperationResult<RatingDto>> SetCommentRatingAsync(CommentRatingDTO commentRatingDto, string userId)
        {
            try
            {
                await _cacheService.RemoveAsync($"comment_{commentRatingDto.CommentId}");
                var result = await _commentRatingRepository.SetCommentRatingAsync(commentRatingDto, userId);
                _logger.LogInformation("Set comment rating for comment {CommentId} by user {UserId} (Success: {Success})", commentRatingDto.CommentId, userId, result.IsSuccess);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting comment rating for comment {CommentId} by user {UserId}", commentRatingDto.CommentId, userId);
                throw;
            }
        }
    }
}