namespace Anekdotify.BL.Interfaces.Services
{
    public interface IJokeCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);

        Task InvalidateJokeAsync(int jokeId);
        Task InvalidateRatingAsync(int jokeId, string userId);
        Task InvalidateViewedJokesAsync(string userId);
        Task InvalidateSavedJokesAsync(int jokeId, string userId);
    }
}
