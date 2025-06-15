using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Anekdotify.BL.Services;

public class UserSavedJokeService(IUserSavedJokeRepository userSavedJokeRepository, IDistributedCache cacheService) : IUserSavedJokeService
{
    public async Task<List<JokePreviewDTO>> GetSavedJokesForUserAsync(string userId)
    {
        var cacheKey = $"saved_jokes_{userId}";
        var cacheValue = await cacheService.GetStringAsync(cacheKey);
        if (cacheValue != null)
        {
            return JsonConvert.DeserializeObject<List<JokePreviewDTO>>(cacheValue) ?? new List<JokePreviewDTO>();
        }
        var savedJokes = await userSavedJokeRepository.GetSavedJokesForUserAsync(userId);

        await cacheService.SetStringAsync(cacheKey, JsonConvert.SerializeObject(savedJokes));

        return savedJokes;
    }

    public async Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO, string userId)
    {
        var cacheKey = $"is_joke_saved_{saveJokeDTO.JokeId}_{userId}";
        var cacheValue = await cacheService.GetStringAsync(cacheKey);

        if (cacheValue != null)
        {
            return bool.Parse(cacheValue);
        }

        var isSaved = await userSavedJokeRepository.IsJokeSavedByUserAsync(saveJokeDTO, userId);

        await cacheService.SetStringAsync(cacheKey, isSaved.ToString());
        return isSaved;
    }

    public async Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
    {

        await cacheService.RemoveAsync($"saved_jokes_{userId}");
        await cacheService.RemoveAsync($"is_joke_saved_{saveJokeDTO.JokeId}_{userId}");

        return await userSavedJokeRepository.RemoveSavedJokeAsync(saveJokeDTO, userId);
    }

    public async Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
    {
        await cacheService.RemoveAsync($"saved_jokes_{userId}");
        await cacheService.RemoveAsync($"is_joke_saved_{saveJokeDTO.JokeId}_{userId}");

        return await userSavedJokeRepository.SaveJokeAsync(saveJokeDTO, userId);
    }
}
