using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class JokeRatingsService(IJokeRatingsRepository jokeRatingsRepository, IJokeCacheService jokeCacheService) : IJokeRatingsService
{
    public async Task<OperationResult<RatingDTO>> GetJokeRatingByUserAsync(int jokeId, string userId)
    {
        var cacheValue = await jokeCacheService.GetStringAsync($"JokeRating_{jokeId}_{userId}");

        if( cacheValue != null )
        {
            return OperationResult<RatingDTO>.Success(JsonConvert.DeserializeObject<RatingDTO>(cacheValue) ?? new RatingDTO());
        }

        var operationRes = await jokeRatingsRepository.GetJokeRatingByUserAsync(jokeId, userId);

        await jokeCacheService.SetStringAsync($"JokeRating_{jokeId}_{userId}", JsonConvert.SerializeObject(operationRes.Value));
       
        return operationRes;
    }

    public async Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId)
    {
        await jokeCacheService.InvalidateRatingAsync(jokeId, userId);

        await jokeCacheService.InvalidateJokeAsync(jokeId);

        return await jokeRatingsRepository.RemoveJokeRatingAsync(jokeId, userId);
    }

    public async Task<OperationResult<RatingDTO>> SetJokeRatingAsync(JokeRatingDTO jokeRatingDTO, string userId)
    {
        await jokeCacheService.InvalidateRatingAsync(jokeRatingDTO.JokeId, userId);

        await jokeCacheService.InvalidateJokeAsync(jokeRatingDTO.JokeId);

        return await jokeRatingsRepository.SetJokeRatingAsync(jokeRatingDTO, userId);
    }
}
