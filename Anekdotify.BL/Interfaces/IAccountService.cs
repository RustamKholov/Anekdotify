using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Interfaces
{
    public interface IAccountService
    {
        Task AddRefreshToken(RefreshToken refreshToken);

        Task<RefreshToken?> GetRefreshTokenByToken(string token);
    }
}
