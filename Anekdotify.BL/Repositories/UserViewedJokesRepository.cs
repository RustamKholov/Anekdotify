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
        public UserViewedJokesRepository(ApplicationDBContext applicationDbContext)
        {
            _context = applicationDbContext;
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

        public async Task<OperationResult<int>> GetLastViewedJokeIdAsync(string userId)
        {
            var lastViewed = await _context.UserViewedJokes
             .Where(uvj => uvj.UserId == userId)
             .OrderByDescending(uvj => uvj.ViewedDate)
             .Select(uvj => uvj.JokeId)
             .FirstOrDefaultAsync();
            if (lastViewed == 0)
            {
                return OperationResult<int>.Fail(lastViewed, "No viewed jokes found");
            }
            return OperationResult<int>.Success(lastViewed);
        }

        public async Task<OperationResult<List<int>>> GetViewedJokesAsync(string userId)
        {
            return OperationResult<List<int>>.Success(await _context.UserViewedJokes
                .Where(uvj => uvj.UserId == userId)
                .Select(uvj => uvj.JokeId)
                .ToListAsync());
        }

        public async Task<OperationResult<bool>> IsLastViewedJokeActualAsync(string userId)
        {
            var lastViewed = await _context.UserViewedJokes
             .Where(uvj => uvj.UserId == userId)
             .OrderByDescending(uvj => uvj.ViewedDate)
             .Select(uvj => uvj.ViewedDate)
             .FirstOrDefaultAsync();
            return OperationResult<bool>.Success(lastViewed.AddHours(24) > DateTime.UtcNow);
        }
    }
}
