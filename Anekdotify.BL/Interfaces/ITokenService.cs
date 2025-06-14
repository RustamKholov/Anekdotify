using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user, bool isRefreshToken);
    }
}