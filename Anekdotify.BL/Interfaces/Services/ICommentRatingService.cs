using Anekdotify.Common;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface ICommentRatingService
    {
        Task<OperationResult<RatingDto>> SetCommentRatingAsync(CommentRatingDTO commentRatingDto, string userId);
        Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId);
        Task<OperationResult<RatingDto>> GetCommentRatingByUserAsync(int commentId, string userId);
    }
}