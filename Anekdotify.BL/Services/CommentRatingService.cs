using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.Extensions.Caching.Distributed;

namespace Anekdotify.BL.Services
{
    public class CommentRatingService(ICommentRatingRepository commentRatingRepository, IDistributedCache cacheService) : ICommentRatingService
    {
        public async Task<OperationResult<RatingDto>> GetCommentRatingByUserAsync(int commentId, string userId)
        {
            return await commentRatingRepository.GetCommentRatingByUserAsync(commentId, userId);
        }

        public async Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId)
        {
            await cacheService.RemoveAsync($"comment_{commentId}");
            return await commentRatingRepository.RemoveCommentRatingAsync(commentId, userId);
        }

        public async Task<OperationResult<RatingDto>> SetCommentRatingAsync(CommentRatingDTO commentRatingDto, string userId)
        {
            await cacheService.RemoveAsync($"comment_{commentRatingDto.CommentId}");
            return await commentRatingRepository.SetCommentRatingAsync(commentRatingDto, userId);
        }
    }
}