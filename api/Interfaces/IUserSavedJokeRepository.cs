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
        Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO);
        Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO);
        Task<List<JokeDTO>> GetSavedJokesForUserAsync(string userId);
        Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO);
    }
}