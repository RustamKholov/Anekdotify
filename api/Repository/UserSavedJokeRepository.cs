using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Common;
using api.Data;
using api.DTOs.Jokes;
using api.DTOs.SaveJoke;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
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
        public async Task<List<JokeDTO>> GetSavedJokesForUserAsync(string userId)
        {
            var userSavedJokes = await _context.UserSavedJokes
                                        .Where(usj => usj.UserId == userId)
                                        .Include(usj => usj.Joke)
                                            .ThenInclude(j => j.Classification)
                                        .Include(usj => usj.Joke)
                                            .ThenInclude(j => j.JokeRatings)
                                        .Include(usj => usj.Joke)
                                            .ThenInclude(j => j.Comments)
                                            .ThenInclude(c => c.User)
                                            .ThenInclude(c => c.CommentRatings)
                                        .Select(usj => usj.Joke)
                                        .ToListAsync();
            return userSavedJokes.Select(j => j.ToJokeDTO()).ToList();
        }

        public async Task<bool> IsJokeSavedByUserAsync(int jokeId, string userId)
        {
            return await _context.UserSavedJokes.AnyAsync(usj => usj.UserId == userId && usj.JokeId == jokeId);
        }

        public async Task<OperationResult> RemoveSavedJokeAsync(int jokeId, string userId)
        {
            var entry = await _context.UserSavedJokes.FirstOrDefaultAsync(suj => suj.UserId == userId && suj.JokeId == jokeId);
            if (entry == null)
            {
                return OperationResult.NotFound("Joke does not saved by user");
            }

            _context.UserSavedJokes.Remove(entry);
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }

        public async Task<OperationResult> SaveJokeAsync(SaveJokeDTO saveJokeDTO, string userId)
        {
            var jokeExists = await _context.Jokes.AnyAsync(j => j.JokeId == saveJokeDTO.JokeId);
            if (!jokeExists)
            {
                return OperationResult.NotFound("Joke not found");
            }
            var existingSavedJoke = await _context.UserSavedJokes.FirstOrDefaultAsync(usj => usj.JokeId == saveJokeDTO.JokeId && usj.UserId == userId);

            if (existingSavedJoke != null)
            {
                return OperationResult.AlreadyExists("Joke is already saved by this user");
            }

            var userSavedJoke = new UserSavedJoke
            {
                JokeId = saveJokeDTO.JokeId,
                UserId = userId
            };

            await _context.UserSavedJokes.AddAsync(userSavedJoke);
            await _context.SaveChangesAsync();

            return OperationResult.Success();
        }
    }
}