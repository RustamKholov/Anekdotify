using Anekdotify.BL.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services
{
    public class JokeCacheService : IJokeCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<JokeCacheService> _logger;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

        public JokeCacheService(IDistributedCache distributedCache, ILogger<JokeCacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                if (value == null)
                {
                    _logger.LogInformation("Cache miss for key {Key}", key);
                    return default;
                }
                _logger.LogInformation("Cache hit for key {Key}", key);
                return System.Text.Json.JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache get failed for key {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(
                    key,
                    json,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
                    });
                _logger.LogInformation("Cache set for key {Key} (expires in {Expiration})", key, expiration ?? DefaultExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache set failed for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogInformation("Cache removed for key {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache remove failed for key {Key}", key);
            }
        }


        public async Task InvalidateJokeAsync(int jokeId)
        {
            await RemoveAsync(JokeCacheKeys.Joke(jokeId));
            await RemoveAsync(JokeCacheKeys.Comments(jokeId));
        }

        public async Task InvalidateRatingAsync(int jokeId, string userId)
        {
            await RemoveAsync(JokeCacheKeys.JokeRating(jokeId, userId));
        }

        public async Task InvalidateSavedJokesAsync(int jokeId, string userId)
        {
            await RemoveAsync(JokeCacheKeys.SavedJokes(userId));
            await RemoveAsync(JokeCacheKeys.IsJokeSaved(jokeId, userId));
        }

        public async Task InvalidateViewedJokesAsync(string userId)
        {
            await RemoveAsync(JokeCacheKeys.LastViewedJoke(userId));
            await RemoveAsync(JokeCacheKeys.ViewedJokes(userId));
            await RemoveAsync(JokeCacheKeys.IsLastViewedJokeActual(userId));
        }
    }

    public static class JokeCacheKeys
    {
        public static string JokesList(int page, int size, bool desc) => $"jokes_page{page}_size{size}_sort{desc}";
        public static string Joke(int id) => $"joke_{id}";
        public static string Comments(int jokeId) => $"comments_joke_{jokeId}";
        public static string JokeRating(int jokeId, string userId) => $"JokeRating_{jokeId}_{userId}";
        public static string SavedJokes(string userId) => $"saved_jokes_{userId}";
        public static string IsJokeSaved(int jokeId, string userId) => $"is_joke_saved_{jokeId}_{userId}";
        public static string LastViewedJoke(string userId) => $"last_viewed_joke_{userId}";
        public static string ViewedJokes(string userId) => $"viewed_jokes_{userId}";
        public static string IsLastViewedJokeActual(string userId) => $"is_last_viewed_joke_actual_{userId}";
    }
}
