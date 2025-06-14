using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(User user, bool isRefreshToken);
    }
}