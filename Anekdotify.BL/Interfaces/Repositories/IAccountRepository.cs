using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task AddRefreshToken(RefreshToken refreshToken);
        Task RemoveRefreshTokenByUserId(string userId);
        Task<RefreshToken?> GetRefreshTokenByToken(string token);
    }
}
