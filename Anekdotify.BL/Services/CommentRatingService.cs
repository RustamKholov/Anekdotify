using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Services
{
    public class CommentRatingService(ICommentRatingRepository commentRatingRepository) : ICommentRatingService
    {
        public async Task<OperationResult<RatingDTO>> GetCommentRatingByUserAsync(int commentId, string userId)
        {
            return await commentRatingRepository.GetCommentRatingByUserAsync(commentId, userId);
        }

        public async Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId)
        {
            return await commentRatingRepository.RemoveCommentRatingAsync(commentId, userId);
        }

        public async Task<OperationResult<RatingDTO>> SetCommentRatingAsync(CommentRatingDTO commentRatingDTO, string userId)
        {
            return await commentRatingRepository.SetCommentRatingAsync(commentRatingDTO, userId);
        }
    }
}