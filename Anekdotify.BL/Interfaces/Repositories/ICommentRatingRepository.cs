using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface ICommentRatingRepository
    {
        Task<OperationResult<RatingDTO>> SetCommentRatingAsync(CommentRatingDTO commentRatingDTO, string userId);
        Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId);
        Task<OperationResult<RatingDTO>> GetCommentRatingByUserAsync(int commentId, string userId);
    }
}