using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services
{
    public class UserViewedJokesService : IUserViewedJokesService
    {
        private readonly IUserViewedJokesRepository _userViewedJokesRepository;
        private readonly IJokeService _jokeService;
        private readonly IJokeCacheService _jokeCacheService;
        private readonly ILogger<UserViewedJokesService> _logger;

        public UserViewedJokesService(
            IUserViewedJokesRepository userViewedJokesRepository,
            IJokeService jokeService,
            IJokeCacheService jokeCacheService,
            ILogger<UserViewedJokesService> logger)
        {
            _userViewedJokesRepository = userViewedJokesRepository;
            _jokeService = jokeService;
            _jokeCacheService = jokeCacheService;
            _logger = logger;
        }

        public async Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId)
        {
            await _jokeCacheService.InvalidateViewedJokesAsync(userId);
            _logger.LogInformation("Invalidated viewed jokes cache for user {UserId}", userId);

            var result = await _userViewedJokesRepository.AddViewedJokeAsync(userId, jokeId);
            _logger.LogInformation("Added viewed joke {JokeId} for user {UserId} (Success: {Success})", jokeId, userId, result.IsSuccess);
            return result;
        }

        public async Task<OperationResult<JokeDto>> GetLastViewedJokeAsync(string userId)
        {
            var cacheKey = JokeCacheKeys.LastViewedJoke(userId);
            var cached = await _jokeCacheService.GetAsync<JokeDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Returned last viewed joke from cache for user {UserId}", userId);
                return OperationResult<JokeDto>.Success(cached);
            }

            var lastJokeId = await _userViewedJokesRepository.GetLastViewedJokeIdAsync(userId);
            if (!lastJokeId.IsSuccess)
            {
                _logger.LogWarning("No last viewed joke found for user {UserId}", userId);
                return OperationResult<JokeDto>.Fail(new JokeDto(), "No last viewed joke found.");
            }

            var jokeDto = await _jokeService.GetJokeByIdAsync(lastJokeId.Value);
            if (jokeDto == null)
            {
                _logger.LogWarning("Joke no longer exists for last viewed joke id {JokeId} and user {UserId}", lastJokeId.Value, userId);
                return OperationResult<JokeDto>.Fail(new JokeDto(), "Joke no longer exists.");
            }

            await _jokeCacheService.SetAsync(cacheKey, jokeDto);
            _logger.LogInformation("Fetched last viewed joke from repository and cached for user {UserId}", userId);

            return OperationResult<JokeDto>.Success(jokeDto);
        }

        public async Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId)
        {
            var cacheKey = JokeCacheKeys.ViewedJokes(userId);
            var cached = await _jokeCacheService.GetAsync<List<int>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Returned viewed jokes from cache for user {UserId}", userId);
                return OperationResult<List<int>>.Success(cached);
            }

            var viewedJokes = await _userViewedJokesRepository.GetViewedJokesAsync(userId);
            if (viewedJokes.IsSuccess && viewedJokes.Value != null)
            {
                await _jokeCacheService.SetAsync(cacheKey, viewedJokes.Value);
                _logger.LogInformation("Fetched viewed jokes from repository and cached for user {UserId}", userId);
                return OperationResult<List<int>>.Success(viewedJokes.Value);
            }
            _logger.LogWarning("Failed to fetch viewed jokes for user {UserId}: {Error}", userId, viewedJokes.ErrorMessage);
            return OperationResult<List<int>>.Fail(new List<int>(), viewedJokes.ErrorMessage!);
        }

        public async Task<OperationResult<bool>> IsLastViewedJokeActualAsync(string userId)
        {
            var cacheKey = JokeCacheKeys.IsLastViewedJokeActual(userId);
            var cached = await _jokeCacheService.GetAsync<bool?>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Returned is-last-viewed-joke-actual from cache for user {UserId}", userId);
                return OperationResult<bool>.Success(cached.Value);
            }

            var res = await _userViewedJokesRepository.IsLastViewedJokeActualAsync(userId);
            if (res.IsSuccess)
            {
                await _jokeCacheService.SetAsync(cacheKey, res.Value);
                _logger.LogInformation("Fetched is-last-viewed-joke-actual from repository and cached for user {UserId}", userId);
                return OperationResult<bool>.Success(res.Value);
            }
            _logger.LogWarning("Failed to fetch is-last-viewed-joke-actual for user {UserId}: {Error}", userId, res.ErrorMessage);
            return OperationResult<bool>.Fail(false, res.ErrorMessage!);
        }
    }
}
