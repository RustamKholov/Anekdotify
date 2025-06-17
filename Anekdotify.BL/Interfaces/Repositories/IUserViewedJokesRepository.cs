using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IUserViewedJokesRepository
    {
        Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId);

        Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId);
        Task<OperationResult<int>> GetLastViewedJokeIdAsync(string userId);
        Task<OperationResult<bool>> IsLastViewedJokeActualAsync(string userId);
    }
}
