using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.SaveJoke;
using Microsoft.Extensions.Caching.Distributed;

namespace Anekdotify.BL.Services
{
    public class JokeCacheService(IDistributedCache distributedCache) : IJokeCacheService
    {
        

        public async Task<string?> GetStringAsync(string key)
        {
            return await distributedCache.GetStringAsync(key);
        }

        public async Task InvalidateJokeAsync(int jokeId)
        {
            await distributedCache.RemoveAsync("list_jokes");
            await distributedCache.RemoveAsync($"joke_{jokeId}");
            await distributedCache.RemoveAsync($"comments_joke_{jokeId}");
        }

        public async Task InvalidateRatingAsync(int jokeId, string userId)
        {
            await distributedCache.RemoveAsync($"JokeRating_{jokeId}_{userId}");
        }

        public async Task InvalidateSavedJokesAsync(int jokeId, string userId)
        {
            await distributedCache.RemoveAsync($"saved_jokes_{userId}");
            await distributedCache.RemoveAsync($"is_joke_saved_{jokeId}_{userId}");
        }

        public async Task InvalidateViewedJokesAsync(string userId)
        {
            await distributedCache.RemoveAsync($"last_viewed_joke_{userId}");
            await distributedCache.RemoveAsync($"viewed_jokes_{userId}");
            await distributedCache.RemoveAsync($"is_last_viewed_joke_actual_{userId}");
        }

        public async Task RemoveAsync(string key)
        {
            await distributedCache.RemoveAsync(key);
        }

        public async Task SetStringAsync(string key, string value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            await distributedCache.SetStringAsync(
                key,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(30)
                });
        }
    }
}
