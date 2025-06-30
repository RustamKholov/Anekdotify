using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;

namespace Anekdotify.BL.Interfaces.Services;

public interface IUserSavedJokeService
{
    Task<OperationResult> SaveJokeAsync(SaveJokeDto saveJokeDto, string userId);
    Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDto saveJokeDto, string userId);
    Task<List<JokeDto>> GetSavedJokesForUserAsync(string userId);
    Task<bool> IsJokeSavedByUserAsync(SaveJokeDto saveJokeDto, string userId);

}
