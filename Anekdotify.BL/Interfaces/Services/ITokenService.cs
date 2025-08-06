using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(User user, bool isRefreshToken = false);
        Task<string> CreateRefreshTokenAsync(User user);
        Task<bool> ValidateRefreshTokenAsync(string token, string userId);
        Task RevokeRefreshTokenAsync(string token);
    }
}