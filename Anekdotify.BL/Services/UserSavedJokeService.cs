using System;
using Anekdotify.BL.Interfaces;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.DTOs.SaveJoke;

namespace Anekdotify.BL.Services;

public class UserSavedJokeService(IUserSavedJokeRepository userSavedJokeRepository) : IUserSavedJokeService
{
    public async Task<List<JokePreviewDTO>> GetSavedJokesForUserAsync(string userId)
    {
        return await userSavedJokeRepository.GetSavedJokesForUserAsync(userId);
    }

    public async Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO, string userId)
    {
        return await userSavedJokeRepository.IsJokeSavedByUserAsync(saveJokeDTO, userId);
    }

    public async Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
    {
        return await userSavedJokeRepository.RemoveSavedJokeAsync(saveJokeDTO, userId);
    }

    public async Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
    {
        return await userSavedJokeRepository.SaveJokeAsync(saveJokeDTO, userId);
    }
}
