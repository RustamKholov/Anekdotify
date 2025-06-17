using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface IJokeCacheService
    {
        Task<string?> GetStringAsync(string key);
        Task SetStringAsync(string key, string value, TimeSpan? absoluteExpirationRelativeToNow = null);
        Task RemoveAsync(string key);
        Task InvalidateJokeAsync(int jokeId);

        Task InvalidateRatingAsync(int jokeId, string userId);
        Task InvalidateViewedJokesAsync(string userId);
        Task InvalidateSavedJokesAsync(int jokeId, string userId);

    }
}
