using Anekdotify.Common;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface IUserViewedJokesService
    {
        Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId);

        Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId);
    }
}
