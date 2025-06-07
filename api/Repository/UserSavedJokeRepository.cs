using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Common;
using api.Data;
using api.DTOs.Jokes;
using api.DTOs.SaveJoke;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class UserSavedJokeRepository : IUserSavedJokeRepository
    {
        private readonly ApplicationDBContext _context;
        public UserSavedJokeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public Task<List<JokeDTO>> GetSavedJokesForUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsJokeSavedByUserAsync(SaveJokeDTO saveJokeDTO)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> RemoveSavedJokeAsync(SaveJokeDTO saveJokeDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO)
        {
            var jokeExists = await _context.Jokes.AnyAsync(j => j.JokeId == saveJokeDTO.JokeId);
            if (!jokeExists)
            {
                return OperationResult.NotFound("Joke not found");
            }
            var existingSavedJoke = await _context.UserSavedJokes.FirstOrDefaultAsync(usj => usj.JokeId == saveJokeDTO.JokeId && usj.UserId == saveJokeDTO.UserId);

            if (existingSavedJoke != null)
            {
                return OperationResult.AlreadyExists("Joke is already saved by this user");
            }

            var userSavedJoke = new UserSavedJoke
            {
                JokeId = saveJokeDTO.JokeId,
                UserId = saveJokeDTO.UserId
            };
            await _context.UserSavedJokes.AddAsync(userSavedJoke);
            await _context.SaveChangesAsync();

            return OperationResult.Success();
        }
    }
}