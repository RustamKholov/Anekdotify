using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class JokeRatingsService(IJokeRatingsRepository jokeRatingsRepository, IDistributedCache cacheService) : IJokeRatingsService
{
    public async Task<OperationResult<RatingDTO>> GetJokeRatingByUserAsync(int jokeId, string userId)
    {
        var cacheValue = await cacheService.GetStringAsync($"JokeRating_{jokeId}_{userId}");

        if(cacheValue != null )
        {
            return OperationResult<RatingDTO>.Success(JsonConvert.DeserializeObject<RatingDTO>(cacheValue) ?? new RatingDTO());
        }

        var operationRes = await jokeRatingsRepository.GetJokeRatingByUserAsync(jokeId, userId);

        await cacheService.SetStringAsync($"JokeRating_{jokeId}_{userId}", JsonConvert.SerializeObject(operationRes.Value));
       
        return operationRes;
    }

    public async Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId)
    {
        await cacheService.RemoveAsync($"JokeRating_{jokeId}_{userId}");
        await cacheService.RemoveAsync("list_jokes");
        await cacheService.RemoveAsync($"joke_{jokeId}");
        await cacheService.RemoveAsync($"comments_joke_{jokeId}");

        return await jokeRatingsRepository.RemoveJokeRatingAsync(jokeId, userId);
    }

    public async Task<OperationResult<RatingDTO>> SetJokeRatingAsync(JokeRatingDTO jokeRatingDTO, string userId)
    {
        await cacheService.RemoveAsync($"JokeRating_{jokeRatingDTO.JokeId}_{userId}");
        await cacheService.RemoveAsync("list_jokes");
        await cacheService.RemoveAsync($"joke_{jokeRatingDTO.JokeId}");
        await cacheService.RemoveAsync($"comments_joke_{jokeRatingDTO.JokeId}");

        return await jokeRatingsRepository.SetJokeRatingAsync(jokeRatingDTO, userId);
    }
}
