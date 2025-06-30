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
        public async Task<OperationResult<RatingDto>> GetCommentRatingByUserAsync(int commentId, string userId)
        {
            var rating = await _context.CommentRatings.FirstOrDefaultAsync(jr => jr.CommentId == commentId && jr.UserId == userId);
            if (rating == null)
            {
                return OperationResult<RatingDto>.Success(new RatingDto { IsLike = null });
            }
            return OperationResult<RatingDto>.Success(new RatingDto { IsLike = rating.Rating });
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

        public async Task<OperationResult<RatingDto>> SetCommentRatingAsync(CommentRatingDTO commentRatingDto, string userId)
        {
            var existingRating = await _context.CommentRatings
                .FirstOrDefaultAsync(jr => jr.CommentId == commentRatingDto.CommentId && jr.UserId == userId);


            if (!commentRatingDto.IsLike.HasValue)
            {
                if (existingRating == null)
                    return OperationResult<RatingDto>.NotFound(new RatingDto
                    {
                        IsLike = null
                    }, "Rating not found to remove.");

                _context.CommentRatings.Remove(existingRating);
                await _context.SaveChangesAsync();
                return OperationResult<RatingDto>.Success(new RatingDto { IsLike = null });
            }


            if (existingRating != null && existingRating.Rating == commentRatingDto.IsLike.Value)
            {
                return OperationResult<RatingDto>.Success(new RatingDto { IsLike = existingRating.Rating });
            }

            if (existingRating == null)
            {
                var newRating = new CommentRating
                {
                    CommentId = commentRatingDto.CommentId,
                    UserId = userId,
                    Rating = commentRatingDto.IsLike.Value
                };
                await _context.CommentRatings.AddAsync(newRating);
            }
            else
            {
                existingRating.Rating = commentRatingDto.IsLike.Value;
            }

            await _context.SaveChangesAsync();
            return OperationResult<RatingDto>.Success(new RatingDto { IsLike = commentRatingDto.IsLike });
        }
    }
}