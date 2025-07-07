using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services;

public class UserSavedJokeService : IUserSavedJokeService
{
    private readonly IUserSavedJokeRepository _userSavedJokeRepository;
    private readonly IJokeService _jokeService;
    private readonly IJokeCacheService _jokeCacheService;
    private readonly ILogger<UserSavedJokeService> _logger;

    public UserSavedJokeService(
        IUserSavedJokeRepository userSavedJokeRepository,
        IJokeService jokeService,
        IJokeCacheService jokeCacheService,
        ILogger<UserSavedJokeService> logger)
    {
        _userSavedJokeRepository = userSavedJokeRepository;
        _jokeService = jokeService;
        _jokeCacheService = jokeCacheService;
        _logger = logger;
    }

    public async Task<List<JokeDto>> GetSavedJokesForUserAsync(string userId)
    {
        var cacheKey = JokeCacheKeys.SavedJokes(userId);
        var cached = await _jokeCacheService.GetAsync<List<JokeDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returned saved jokes from cache for user {UserId}", userId);
            return cached;
        }

        var savedJokesIds = await _userSavedJokeRepository.GetSavedJokesForUserAsync(userId);
        var savedJokes = await _jokeService.GetJokesByIdsAsync(savedJokesIds);

        await _jokeCacheService.SetAsync(cacheKey, savedJokes);
        _logger.LogInformation("Fetched saved jokes from repository and cached for user {UserId}", userId);

        return savedJokes;
    }

    public async Task<bool> IsJokeSavedByUserAsync(SaveJokeDto saveJokeDto, string userId)
    {
        var cacheKey = JokeCacheKeys.IsJokeSaved(saveJokeDto.JokeId, userId);
        var cached = await _jokeCacheService.GetAsync<bool?>(cacheKey);
        if (cached.HasValue)
        {
            _logger.LogInformation("Returned is-joke-saved from cache for joke {JokeId} and user {UserId}", saveJokeDto.JokeId, userId);
            return cached.Value;
        }

        var isSaved = await _userSavedJokeRepository.IsJokeSavedByUserAsync(saveJokeDto, userId);
        await _jokeCacheService.SetAsync(cacheKey, isSaved);
        _logger.LogInformation("Fetched is-joke-saved from repository and cached for joke {JokeId} and user {UserId}", saveJokeDto.JokeId, userId);

        return isSaved;
    }

    public async Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDto saveJokeDto, string userId)
    {
        await _jokeCacheService.InvalidateSavedJokesAsync(saveJokeDto.JokeId, userId);
        _logger.LogInformation("Invalidated saved jokes cache for joke {JokeId} and user {UserId}", saveJokeDto.JokeId, userId);

        var result = await _userSavedJokeRepository.RemoveSavedJokeAsync(saveJokeDto, userId);
        _logger.LogInformation("Removed saved joke for joke {JokeId} and user {UserId} (Success: {Success})", saveJokeDto.JokeId, userId, result.IsSuccess);
        return result;
    }

    public async Task<OperationResult> SaveJokeAsync(SaveJokeDto saveJokeDto, string userId)
    {
        await _jokeCacheService.InvalidateSavedJokesAsync(saveJokeDto.JokeId, userId);
        _logger.LogInformation("Invalidated saved jokes cache for joke {JokeId} and user {UserId}", saveJokeDto.JokeId, userId);

        var result = await _userSavedJokeRepository.SaveJokeAsync(saveJokeDto, userId);
        _logger.LogInformation("Saved joke for joke {JokeId} and user {UserId} (Success: {Success})", saveJokeDto.JokeId, userId, result.IsSuccess);
        return result;
    }
}
