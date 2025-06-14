using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.Database.Data;
using Anekdotify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anekdotify.BL.Repositories
{
    public class AccountRepository(ApplicationDBContext context) : IAccountRepository
    {
        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            await context.RefreshTokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenByToken(string token)
        {
            return await context.RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RemoveRefreshTokenByUserId(string userId)
        {
            var existingToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId);
            if (existingToken != null)
            {
                context.RemoveRange(existingToken);
            }
            await context.SaveChangesAsync();
        }
    }
}
