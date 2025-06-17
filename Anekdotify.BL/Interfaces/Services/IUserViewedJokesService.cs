using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface IUserViewedJokesService
    {
        Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId);

        Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId);

        Task<OperationResult<JokeDTO>> GetLastViewedJokeAsync(string userId);
        Task<OperationResult<bool>> IsLastViewedJokeActualAsync(string userId);
    }
}
