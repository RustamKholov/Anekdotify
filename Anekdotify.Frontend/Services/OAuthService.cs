using Anekdotify.Frontend.Authentication;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using System.Text;

namespace Anekdotify.Frontend.Services
{
    public interface IOAuthService
    {
        Task<OAuthUrlResponse?> GetGitHubLoginUrlAsync();
        Task<OAuthUrlResponse?> GetGoogleLoginUrlAsync();
        Task<LoginResponseModel?> ExchangeGitHubCodeAsync(string code, string state);
        Task<LoginResponseModel?> ExchangeGoogleCodeAsync(string code, string state);
        Task HandleOAuthCallbackAsync(string token, string refreshToken, long expiresIn);
    }

    public class OAuthService : IOAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<OAuthService> _logger;
        private readonly HttpClient _httpClient;

        public OAuthService(
            IConfiguration configuration,
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager,
            ILogger<OAuthService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<OAuthUrlResponse?> GetGitHubLoginUrlAsync()
        {
            try
            {
                var apiUrl = _configuration["JokeStoreApiUrl"];
                var response = await _httpClient.GetAsync($"{apiUrl}/account/oauth/github/url");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<OAuthUrlResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                _logger.LogError("Failed to get GitHub OAuth URL: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GitHub OAuth URL");
                return null;
            }
        }

        public async Task<OAuthUrlResponse?> GetGoogleLoginUrlAsync()
        {
            try
            {
                var apiUrl = _configuration["JokeStoreApiUrl"];
                var response = await _httpClient.GetAsync($"{apiUrl}/account/oauth/google/url");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<OAuthUrlResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                _logger.LogError("Failed to get Google OAuth URL: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google OAuth URL");
                return null;
            }
        }

        public async Task<LoginResponseModel?> ExchangeGitHubCodeAsync(string code, string state)
        {
            try
            {
                var apiUrl = _configuration["JokeStoreApiUrl"];
                var request = new { Code = code, State = state };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiUrl}/account/oauth/github/exchange", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LoginResponseModel>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                _logger.LogError("Failed to exchange GitHub code: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging GitHub code");
                return null;
            }
        }

        public async Task<LoginResponseModel?> ExchangeGoogleCodeAsync(string code, string state)
        {
            try
            {
                var apiUrl = _configuration["JokeStoreApiUrl"];
                var request = new { Code = code, State = state };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiUrl}/account/oauth/google/exchange", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<LoginResponseModel>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                _logger.LogError("Failed to exchange Google code: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging Google code");
                return null;
            }
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

    public class OAuthUrlResponse
    {
        public string Url { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}