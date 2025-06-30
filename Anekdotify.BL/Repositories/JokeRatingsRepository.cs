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
        public readonly ApplicationDBContext Context;
        public JokeRatingsRepository(ApplicationDBContext context)
        {
            Context = context;
        }
        public async Task<OperationResult<RatingDto>> GetJokeRatingByUserAsync(int jokeId, string userId)
        {
            var rating = await Context.JokeRatings.FirstOrDefaultAsync(jr => jr.JokeId == jokeId && jr.UserId == userId);
            if (rating == null)
            {
                return OperationResult<RatingDto>.Success(new RatingDto { IsLike = null });
            }
            return OperationResult<RatingDto>.Success(new RatingDto { IsLike = rating.Rating });
        }

        public async Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId)
        {
            var existingRating = await Context.JokeRatings
                                   .FirstOrDefaultAsync(jr => jr.JokeId == jokeId && jr.UserId == userId);

            if (existingRating != null)
            {
                Context.JokeRatings.Remove(existingRating);
            }
            else
            {
                return OperationResult.NotFound("Rating not found to remove.");
            }
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult<RatingDto>> SetJokeRatingAsync(JokeRatingDto jokeRatingDto, string userId)
        {
            var existingRating = await Context.JokeRatings
                .FirstOrDefaultAsync(jr => jr.JokeId == jokeRatingDto.JokeId && jr.UserId == userId);

            
            if (!jokeRatingDto.IsLike.HasValue)
            {
                if (existingRating == null)
                    return OperationResult<RatingDto>.NotFound(new RatingDto
                    {
                        IsLike = null
                    }, "Rating not found to remove.");

                Context.JokeRatings.Remove(existingRating);
                await Context.SaveChangesAsync();
                return OperationResult<RatingDto>.Success(new RatingDto { IsLike = null });
            }

            
            if (existingRating != null && existingRating.Rating == jokeRatingDto.IsLike.Value)
            {
                return OperationResult<RatingDto>.Success(new RatingDto { IsLike = existingRating.Rating });
            }

            if (existingRating == null)
            {
                var newRating = new JokeRating
                {
                    JokeId = jokeRatingDto.JokeId,
                    UserId = userId,
                    Rating = jokeRatingDto.IsLike.Value
                };
                await Context.JokeRatings.AddAsync(newRating);
            }
            else
            {
                existingRating.Rating = jokeRatingDto.IsLike.Value;
            }

            await Context.SaveChangesAsync();
            return OperationResult<RatingDto>.Success(new RatingDto { IsLike = jokeRatingDto.IsLike });
        }
    }
}