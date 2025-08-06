using System.Security.Claims;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IAccountService _accountService;
        private readonly ILogger<OAuthController> _logger;
        private readonly IConfiguration _configuration;

        public OAuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            IAccountService accountService,
            ILogger<OAuthController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _accountService = accountService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("github")]
        public IActionResult LoginWithGitHub(string? returnUrl = null)
        {
            _logger.LogInformation("Initiating GitHub OAuth login");

            var redirectUrl = Url.Action(nameof(GitHubCallback), "OAuth", new { returnUrl });
            _logger.LogInformation("GitHub redirect URL: {RedirectUrl}", redirectUrl);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", redirectUrl);
            return Challenge(properties, "GitHub");
        }

        [HttpGet("google")]
        public IActionResult LoginWithGoogle(string? returnUrl = null)
        {
            _logger.LogInformation("Initiating Google OAuth login");

            var redirectUrl = Url.Action(nameof(GoogleCallback), "OAuth", new { returnUrl });
            _logger.LogInformation("Google redirect URL: {RedirectUrl}", redirectUrl);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            _logger.LogInformation("GitHub callback method called");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("GitHub external login info was null");
                var errorUrl = $"{_configuration["Frontend:BaseUrl"]}/login?error=external_login_failed";
                _logger.LogInformation("Redirecting to error URL: {ErrorUrl}", errorUrl);
                return Redirect(errorUrl);
            }

            _logger.LogInformation("GitHub callback - Processing external login for provider: {Provider}, key: {ProviderKey}",
                info.LoginProvider, info.ProviderKey);

            var result = await ProcessExternalLogin(info, "GitHub");

            if (result.IsSuccess)
            {
                var successUrl = $"{_configuration["Frontend:BaseUrl"]}/auth/callback?token={result.Token}&refreshToken={result.RefreshToken}&expiresIn={result.ExpiresIn}";
                _logger.LogInformation("GitHub OAuth successful, redirecting to: {SuccessUrl}", successUrl);
                return Redirect(successUrl);
            }

            var failureUrl = $"{_configuration["Frontend:BaseUrl"]}/login?error={result.Error}";
            _logger.LogError("GitHub OAuth failed with error: {Error}, redirecting to: {FailureUrl}", result.Error, failureUrl);
            return Redirect(failureUrl);
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            _logger.LogInformation("Google callback method called");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Google external login info was null");
                var errorUrl = $"{_configuration["Frontend:BaseUrl"]}/login?error=external_login_failed";
                _logger.LogInformation("Redirecting to error URL: {ErrorUrl}", errorUrl);
                return Redirect(errorUrl);
            }

            _logger.LogInformation("Google callback - Processing external login for provider: {Provider}, key: {ProviderKey}",
                info.LoginProvider, info.ProviderKey);

            var result = await ProcessExternalLogin(info, "Google");

            if (result.IsSuccess)
            {
                var successUrl = $"{_configuration["Frontend:BaseUrl"]}/auth/callback?token={result.Token}&refreshToken={result.RefreshToken}&expiresIn={result.ExpiresIn}";
                _logger.LogInformation("Google OAuth successful, redirecting to: {SuccessUrl}", successUrl);
                return Redirect(successUrl);
            }

            var failureUrl = $"{_configuration["Frontend:BaseUrl"]}/login?error={result.Error}";
            _logger.LogError("Google OAuth failed with error: {Error}, redirecting to: {FailureUrl}", result.Error, failureUrl);
            return Redirect(failureUrl);
        }

        private async Task<OAuthResult> ProcessExternalLogin(ExternalLoginInfo info, string provider)
        {
            try
            {
                // Extract user information from claims
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var externalId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get avatar URL based on provider
                var avatar = provider switch
                {
                    "GitHub" => info.Principal.FindFirstValue("urn:github:avatar_url"),
                    "Google" => info.Principal.FindFirstValue("picture"),
                    _ => null
                };

                _logger.LogInformation("Processing {Provider} login for email: {Email}, externalId: {ExternalId}",
                    provider, email, externalId);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(externalId))
                {
                    _logger.LogWarning("Missing email or external ID from {Provider}", provider);
                    return OAuthResult.Failure("missing_user_info");
                }

                // Check if user already exists with this external login
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (user == null)
                {
                    // Check if user exists with this email
                    user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        // Create new user
                        user = new User
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                            RegistrationDate = DateTime.UtcNow,
                            Provider = provider,
                            Avatar = avatar
                        };

                        // Set provider-specific ID
                        if (provider == "GitHub")
                            user.GitHubId = externalId;
                        else if (provider == "Google")
                            user.GoogleId = externalId;

                        var createResult = await _userManager.CreateAsync(user);
                        if (!createResult.Succeeded)
                        {
                            _logger.LogError("Failed to create user from {Provider}: {@Errors}",
                                provider, createResult.Errors);
                            return OAuthResult.Failure("user_creation_failed");
                        }

                        // Add to User role
                        await _userManager.AddToRoleAsync(user, "User");
                        _logger.LogInformation("Created new user from {Provider}: {Email}", provider, email);
                    }

                    // Add external login
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        _logger.LogError("Failed to add external login for {Provider}: {@Errors}",
                            provider, addLoginResult.Errors);
                        return OAuthResult.Failure("external_login_failed");
                    }
                }

                // Update user info
                var updateNeeded = false;

                if (user.Avatar != avatar && !string.IsNullOrEmpty(avatar))
                {
                    user.Avatar = avatar;
                    updateNeeded = true;
                }

                if (provider == "GitHub" && user.GitHubId != externalId)
                {
                    user.GitHubId = externalId;
                    updateNeeded = true;
                }
                else if (provider == "Google" && user.GoogleId != externalId)
                {
                    user.GoogleId = externalId;
                    updateNeeded = true;
                }

                user.LastLoginDate = DateTime.UtcNow;
                updateNeeded = true;

                if (updateNeeded)
                {
                    await _userManager.UpdateAsync(user);
                }

                // Clear existing refresh tokens for this user
                await _accountService.RemoveRefreshTokenByUserId(user.Id);

                // Generate tokens using the same pattern as AccountController
                var token = _tokenService.CreateToken(user, isRefreshToken: false);
                var refreshToken = _tokenService.CreateToken(user, isRefreshToken: true);

                // Save refresh token using IAccountService
                await _accountService.AddRefreshToken(new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                });

                _logger.LogInformation("User {Email} logged in successfully via {Provider}", email, provider);

                var expiresIn = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
                return OAuthResult.Success(token, refreshToken, expiresIn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing external login from {Provider}", provider);
                return OAuthResult.Failure("internal_error");
            }
        }

        private class OAuthResult
        {
            public bool IsSuccess { get; set; }
            public string? Token { get; set; }
            public string? RefreshToken { get; set; }
            public long ExpiresIn { get; set; }
            public string? Error { get; set; }

            public static OAuthResult Success(string token, string refreshToken, long expiresIn)
                => new() { IsSuccess = true, Token = token, RefreshToken = refreshToken, ExpiresIn = expiresIn };

            public static OAuthResult Failure(string error)
                => new() { IsSuccess = false, Error = error };
        }
    }
}