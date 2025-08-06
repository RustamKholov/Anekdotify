using Anekdotify.Frontend.Authentication;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Anekdotify.Frontend.Services
{
    public interface IOAuthService
    {
        Task<string> GetGitHubLoginUrlAsync();
        Task<string> GetGoogleLoginUrlAsync();
        Task HandleOAuthCallbackAsync(string token, string refreshToken, long expiresIn);
    }

    public class OAuthService : IOAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<OAuthService> _logger;

        public OAuthService(
            IConfiguration configuration,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            ILogger<OAuthService> logger)
        {
            _configuration = configuration;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
            _logger = logger;
        }

        public Task<string> GetGitHubLoginUrlAsync()
        {
            var apiUrl = _configuration["JokeStoreApiUrl"];
            return Task.FromResult($"{apiUrl}/api/auth/github");
        }

        public Task<string> GetGoogleLoginUrlAsync()
        {
            var apiUrl = _configuration["JokeStoreApiUrl"];
            return Task.FromResult($"{apiUrl}/api/auth/google");
        }

        public async Task HandleOAuthCallbackAsync(string token, string refreshToken, long expiresIn)
        {
            try
            {
                var loginResponse = new LoginResponseModel
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresIn = expiresIn
                };

                var customAuthProvider = (CustomAuthStateProvider)_authenticationStateProvider;
                await customAuthProvider.MarkUserAsAuthenticated(loginResponse);

                _logger.LogInformation("OAuth callback handled successfully");
                _navigationManager.NavigateTo("/", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OAuth callback");
                _navigationManager.NavigateTo("/login?error=callback_error");
            }
        }
    }
}