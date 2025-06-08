using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Common;
using api.DTOs.Jokes;
using api.DTOs.SaveJoke;

namespace api.Interfaces
{
    public interface IUserSavedJokeRepository
    {
        Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId);
        Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO, string userId);
        Task<List<JokePreviewDTO>> GetSavedJokesForUserAsync(string userId);
        Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO, string userId);
    }
}