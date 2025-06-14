using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface IAccountService
    {
        Task AddRefreshToken(RefreshToken refreshToken);

        Task<RefreshToken?> GetRefreshTokenByToken(string token);
    }
}
