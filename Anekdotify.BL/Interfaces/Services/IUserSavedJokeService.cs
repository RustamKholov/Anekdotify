using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;

namespace Anekdotify.BL.Interfaces.Services;

public interface IUserSavedJokeService
{
    Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId);
    Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO, string userId);
    Task<List<JokeDTO>> GetSavedJokesForUserAsync(string userId);
    Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO, string userId);

}
