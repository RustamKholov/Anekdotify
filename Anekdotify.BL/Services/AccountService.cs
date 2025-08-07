using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Anekdotify.BL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            try
            {
                await _accountRepository.RemoveRefreshTokenByUserId(refreshToken.UserId);
                await _accountRepository.AddRefreshToken(refreshToken);

                _logger.LogInformation("Refresh token added for user {UserId}", refreshToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add refresh token for user {UserId}", refreshToken.UserId);

                throw;
            }
        }

        public async Task<RefreshToken?> GetRefreshTokenByToken(string token)
        {
            try
            {
                var result = await _accountRepository.GetRefreshTokenByToken(token);

                _logger.LogInformation("Refresh token lookup for token {Token}: {Result}", token, result != null ? "Found" : "Not found");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get refresh token for token {Token}", token);

                throw;
            }
        }
        public async Task RemoveRefreshTokenByUserId(string userId)
        {
            try
            {
                await _accountRepository.RemoveRefreshTokenByUserId(userId);

                _logger.LogInformation("Refresh token removed for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove refresh token for user {UserId}", userId);

                throw;
            }
        }
    }
}
