using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.Common;
using Anekdotify.Database.Data;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class JokeRatingsRepository : IJokeRatingsRepository
    {
        public readonly ApplicationDBContext _context;
        public JokeRatingsRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<OperationResult<RatingDTO>> GetJokeRatingByUserAsync(int jokeId, string userId)
        {
            var rating = await _context.JokeRatings.FirstOrDefaultAsync(jr => jr.JokeId == jokeId && jr.UserId == userId);
            if (rating == null)
            {
                return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = null });
            }
            return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = rating.Rating });
        }

        public async Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId)
        {
            var existingRating = await _context.JokeRatings
                                   .FirstOrDefaultAsync(jr => jr.JokeId == jokeId && jr.UserId == userId);

            if (existingRating != null)
            {
                _context.JokeRatings.Remove(existingRating);
            }
            else
            {
                return OperationResult.NotFound("Rating not found to remove.");
            }
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult<RatingDTO>> SetJokeRatingAsync(JokeRatingDTO jokeRatingDTO, string userId)
        {
            var existingRating = await _context.JokeRatings
                .FirstOrDefaultAsync(jr => jr.JokeId == jokeRatingDTO.JokeId && jr.UserId == userId);

            
            if (!jokeRatingDTO.IsLike.HasValue)
            {
                if (existingRating == null)
                    return OperationResult<RatingDTO>.NotFound(new RatingDTO { }, "Rating not found to remove.");

                _context.JokeRatings.Remove(existingRating);
                await _context.SaveChangesAsync();
                return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = null });
            }

            
            if (existingRating != null && existingRating.Rating == jokeRatingDTO.IsLike.Value)
            {
                return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = existingRating.Rating });
            }

            if (existingRating == null)
            {
                var newRating = new JokeRating
                {
                    JokeId = jokeRatingDTO.JokeId,
                    UserId = userId,
                    Rating = jokeRatingDTO.IsLike.Value
                };
                await _context.JokeRatings.AddAsync(newRating);
            }
            else
            {
                existingRating.Rating = jokeRatingDTO.IsLike.Value;
            }

            await _context.SaveChangesAsync();
            return OperationResult<RatingDTO>.Success(new RatingDTO { IsLike = jokeRatingDTO.IsLike });
        }
    }
}