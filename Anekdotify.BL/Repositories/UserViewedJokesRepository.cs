using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.Common;
using Anekdotify.Database.Data;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class UserViewedJokesRepository : IUserViewedJokesRepository
    {
        private readonly ApplicationDBContext _context;
        public UserViewedJokesRepository(ApplicationDBContext applicationDBContext)
        {
            _context = applicationDBContext;
        }

        public async Task<OperationResult> AddViewedJokeAsync(string userId, int jokeId)
        {
            await _context.UserViewedJokes.AddAsync(new UserViewedJoke
            {
                UserId = userId,
                JokeId = jokeId
            });
            await _context.SaveChangesAsync();
            return OperationResult.Success();
        }


        public async Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId)
        {
            return OperationResult<List<int>>.Success(await _context.UserViewedJokes
                .Where(uvj => uvj.UserId == userId)
                .Select(uvj => uvj.JokeId)
                .ToListAsync());
        }
    }
}
