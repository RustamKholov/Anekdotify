using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IUserSavedJokeRepository
    {
        Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId);
        Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO, string userId);
        Task<List<JokePreviewDTO>> GetSavedJokesForUserAsync(string userId);
        Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO, string userId);
    }
}