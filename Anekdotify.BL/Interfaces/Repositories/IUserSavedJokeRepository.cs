using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IUserSavedJokeRepository
    {
        Task<OperationResult> SaveJokeAsync(SaveJokeDto saveJokeDTO, string userId);
        Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDto saveJokeDTO, string userId);
        Task<List<int>> GetSavedJokesForUserAsync(string userId);
        Task<bool> IsJokeSavedByUserAsync(SaveJokeDto saveJokeDTO, string userId);
    }
}