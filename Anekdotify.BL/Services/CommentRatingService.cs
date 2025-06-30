using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Services
{
    public class CommentRatingService(ICommentRatingRepository commentRatingRepository) : ICommentRatingService
    {
        public async Task<OperationResult<RatingDto>> GetCommentRatingByUserAsync(int commentId, string userId)
        {
            return await commentRatingRepository.GetCommentRatingByUserAsync(commentId, userId);
        }

        public async Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId)
        {
            return await commentRatingRepository.RemoveCommentRatingAsync(commentId, userId);
        }

        public async Task<OperationResult<RatingDto>> SetCommentRatingAsync(CommentRatingDTO commentRatingDto, string userId)
        {
            return await commentRatingRepository.SetCommentRatingAsync(commentRatingDto, userId);
        }
    }
}