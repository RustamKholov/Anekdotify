using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class JokeRatingsService(IJokeRatingsRepository jokeRatingsRepository, IJokeCacheService jokeCacheService) : IJokeRatingsService
{
    public async Task<OperationResult<RatingDto>> GetJokeRatingByUserAsync(int jokeId, string userId)
    {
        var cacheValue = await jokeCacheService.GetStringAsync($"JokeRating_{jokeId}_{userId}");

        if( cacheValue != null )
        {
            return OperationResult<RatingDto>.Success(JsonConvert.DeserializeObject<RatingDto>(cacheValue) ?? new RatingDto());
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

    public async Task<OperationResult<RatingDto>> SetJokeRatingAsync(JokeRatingDto jokeRatingDto, string userId)
    {
        await jokeCacheService.InvalidateRatingAsync(jokeRatingDto.JokeId, userId);

        await jokeCacheService.InvalidateJokeAsync(jokeRatingDto.JokeId);

        return await jokeRatingsRepository.SetJokeRatingAsync(jokeRatingDto, userId);
    }
}
