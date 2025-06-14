using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;

namespace Anekdotify.BL.Services;

public class JokeRatingsService(IJokeRatingsRepository jokeRatingsRepository) : IJokeRatingsService
{
    public async Task<OperationResult<RatingDTO>> GetJokeRatingByUserAsync(int jokeId, string userId)
    {
        return await jokeRatingsRepository.GetJokeRatingByUserAsync(jokeId, userId);
    }

    public async Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId)
    {
        return await jokeRatingsRepository.RemoveJokeRatingAsync(jokeId, userId);
    }

    public async Task<OperationResult<RatingDTO>> SetJokeRatingAsync(JokeRatingDTO jokeRatingDTO, string userId)
    {
        return await jokeRatingsRepository.SetJokeRatingAsync(jokeRatingDTO, userId);
    }
}
