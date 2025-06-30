using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class UserSavedJokeService(IUserSavedJokeRepository userSavedJokeRepository, IJokeService jokeService, IJokeCacheService jokecacheService) : IUserSavedJokeService
{
    public async Task<List<JokeDto>> GetSavedJokesForUserAsync(string userId)
    {
        var cacheKey = $"saved_jokes_{userId}";
        var cacheValue = await jokecacheService.GetStringAsync(cacheKey);
        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<List<JokeDto>>(cacheValue) ?? new List<JokeDto>();
        }
        var savedJokesIds = await userSavedJokeRepository.GetSavedJokesForUserAsync(userId);

        var savedJokes = await jokeService.GetJokesByIdsAsync(savedJokesIds);

        await jokecacheService.SetStringAsync(cacheKey, JsonConvert.SerializeObject(savedJokes));

        return savedJokes;
    }

    public async Task<bool> IsJokeSavedByUserAsync(SaveJokeDto saveJokeDto, string userId)
    {
        var cacheKey = $"is_joke_saved_{saveJokeDto.JokeId}_{userId}";
        var cacheValue = await jokecacheService.GetStringAsync(cacheKey);

        if (cacheValue != null)
        {
            return bool.Parse(cacheValue);
        }

        var isSaved = await userSavedJokeRepository.IsJokeSavedByUserAsync(saveJokeDto, userId);

        await jokecacheService.SetStringAsync(cacheKey, isSaved.ToString());
        return isSaved;
    }

    public async Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDto saveJokeDto, string userId)
    {

        await jokecacheService.InvalidateSavedJokesAsync(saveJokeDto.JokeId, userId);

        return await userSavedJokeRepository.RemoveSavedJokeAsync(saveJokeDto, userId);
    }

    public async Task<OperationResult> SaveJokeAsync(SaveJokeDto saveJokeDto, string userId)
    {
        await jokecacheService.InvalidateSavedJokesAsync(saveJokeDto.JokeId, userId);

        return await userSavedJokeRepository.SaveJokeAsync(saveJokeDto, userId);
    }
}
