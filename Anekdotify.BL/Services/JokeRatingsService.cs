using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.JokeRating;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services;

public class JokeRatingsService : IJokeRatingsService
{
    private readonly IJokeRatingsRepository _jokeRatingsRepository;
    private readonly IJokeCacheService _jokeCacheService;
    private readonly ILogger<JokeRatingsService> _logger;

    public JokeRatingsService(
        IJokeRatingsRepository jokeRatingsRepository,
        IJokeCacheService jokeCacheService,
        ILogger<JokeRatingsService> logger)
    {
        _jokeRatingsRepository = jokeRatingsRepository;
        _jokeCacheService = jokeCacheService;
        _logger = logger;
    }

    public async Task<OperationResult<RatingDto>> GetJokeRatingByUserAsync(int jokeId, string userId)
    {
        var cacheKey = JokeCacheKeys.JokeRating(jokeId, userId);
        var cached = await _jokeCacheService.GetAsync<RatingDto>(cacheKey);

        if (cached != null)
        {
            _logger.LogInformation("Returned joke rating from cache for joke {JokeId} and user {UserId}", jokeId, userId);
            return OperationResult<RatingDto>.Success(cached);
        }

        var operationRes = await _jokeRatingsRepository.GetJokeRatingByUserAsync(jokeId, userId);

        if (operationRes.Value != null)
        {
            await _jokeCacheService.SetAsync(cacheKey, operationRes.Value);
            _logger.LogInformation("Fetched joke rating from repository and cached for joke {JokeId} and user {UserId}", jokeId, userId);
        }
        else
        {
            _logger.LogWarning("Joke rating not found for joke {JokeId} and user {UserId}", jokeId, userId);
        }

        return operationRes;
    }

    public async Task<OperationResult> RemoveJokeRatingAsync(int jokeId, string userId)
    {
        await _jokeCacheService.InvalidateRatingAsync(jokeId, userId);
        await _jokeCacheService.InvalidateJokeAsync(jokeId);
        _logger.LogInformation("Invalidated rating and joke cache for joke {JokeId} and user {UserId}", jokeId, userId);

        var result = await _jokeRatingsRepository.RemoveJokeRatingAsync(jokeId, userId);
        _logger.LogInformation("Removed joke rating for joke {JokeId} and user {UserId} (Success: {Success})", jokeId, userId, result.IsSuccess);
        return result;
    }

    public async Task<OperationResult<RatingDto>> SetJokeRatingAsync(JokeRatingDto jokeRatingDto, string userId)
    {
        await _jokeCacheService.InvalidateRatingAsync(jokeRatingDto.JokeId, userId);
        await _jokeCacheService.InvalidateJokeAsync(jokeRatingDto.JokeId);
        _logger.LogInformation("Invalidated rating and joke cache for joke {JokeId} and user {UserId}", jokeRatingDto.JokeId, userId);

        var result = await _jokeRatingsRepository.SetJokeRatingAsync(jokeRatingDto, userId);
        _logger.LogInformation("Set joke rating for joke {JokeId} and user {UserId} (Success: {Success})", jokeRatingDto.JokeId, userId, result.IsSuccess);
        return result;
    }
}
