using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.Common;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.CommentRating;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class CommentRatingRepository : ICommentRatingRepository
    {
        private readonly ApplicationDBContext _context;
        public CommentRatingRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<OperationResult<RatingDTO>> GetCommentRatingByUserAsync(int commentId, string userId)
        {
            var rating = await _context.CommentRatings.FirstOrDefaultAsync(jr => jr.CommentId == commentId && jr.UserId == userId);
            if (rating == null)
            {
                return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = null });
            }
            return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = rating.Rating });
        }

        public async Task<OperationResult> RemoveCommentRatingAsync(int commentId, string userId)
        {
            var existingRating = await _context.CommentRatings
                                   .FirstOrDefaultAsync(jr => jr.CommentId == commentId && jr.UserId == userId);

            if (existingRating != null)
            {
                _context.CommentRatings.Remove(existingRating);
            }
            else
            {
                return OperationResult.NotFound("Rating not found to remove.");
            }
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult<RatingDTO>> SetCommentRatingAsync(CommentRatingDTO commentRatingDTO, string userId)
        {
            var existingRating = await _context.CommentRatings
                .FirstOrDefaultAsync(jr => jr.CommentId == commentRatingDTO.CommentId && jr.UserId == userId);


            if (!commentRatingDTO.IsLike.HasValue)
            {
                if (existingRating == null)
                    return OperationResult<RatingDTO>.NotFound(new RatingDTO { }, "Rating not found to remove.");

                _context.CommentRatings.Remove(existingRating);
                await _context.SaveChangesAsync();
                return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = null });
            }


            if (existingRating != null && existingRating.Rating == commentRatingDTO.IsLike.Value)
            {
                return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = existingRating.Rating });
            }

            if (existingRating == null)
            {
                var newRating = new CommentRating
                {
                    CommentId = commentRatingDTO.CommentId,
                    UserId = userId,
                    Rating = commentRatingDTO.IsLike.Value
                };
                await _context.CommentRatings.AddAsync(newRating);
            }
            else
            {
                existingRating.Rating = commentRatingDTO.IsLike.Value;
            }

            await _context.SaveChangesAsync();
            return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = commentRatingDTO.IsLike });
        }
    }
}