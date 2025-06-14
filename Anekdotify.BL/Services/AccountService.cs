using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anekdotify.BL.Interfaces;
using Anekdotify.Models.Entities;

namespace Anekdotify.BL.Services
{
    public class AccountService(IAccountRepository accountRepository) : IAccountService
    {
        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            await accountRepository.RemoveRefreshTokenByUserId(refreshToken.UserId);

            await accountRepository.AddRefreshToken(refreshToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenByToken(string token)
        {
            return await accountRepository.GetRefreshTokenByToken(token);
        }
    }
}
