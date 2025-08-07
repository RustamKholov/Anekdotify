using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace Anekdotify.Api.Controllers
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<User> userManager,
            ITokenService tokenService,
            SignInManager<User> signInManager,
            IAccountService accountService,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _accountService = accountService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt: {@LoginDto}", loginDto);
                return BadRequest(loginDto);
            }

            User? user = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.UserName == loginDto.Username || u.Email == loginDto.Username);

            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for {Username}", loginDto.Username);
                return Unauthorized("Username not found and/or wrong password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed: invalid password for {Username}", loginDto.Username);
                return Unauthorized("Username not found and/or wrong password");
            }

            _logger.LogInformation("User {Username} logged in successfully.", loginDto.Username);

            var token = _tokenService.CreateToken(user, isRefreshToken: false);
            var refreshToken = _tokenService.CreateToken(user, isRefreshToken: true);

            await _accountService.AddRefreshToken(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken
            });

            var loginResponse = new LoginResponseModel
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            };

            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(loginResponse);
        }

        [HttpGet("loginByRefreshToken")]
        public async Task<ActionResult<LoginResponseModel>> LoginByRefreshToken([FromQuery] string refreshToken)
        {
            var refreshTokenEntity = await _accountService.GetRefreshTokenByToken(refreshToken);
            if (refreshTokenEntity == null)
            {
                _logger.LogWarning("Invalid or expired refresh token used: {RefreshToken}", refreshToken);
                return Unauthorized("Invalid or expired refresh token.");
            }

            _logger.LogInformation("Refresh token used for user {UserId}", refreshTokenEntity.UserId);

            var token = _tokenService.CreateToken(refreshTokenEntity.User, isRefreshToken: false);
            var newRefreshToken = _tokenService.CreateToken(refreshTokenEntity.User, isRefreshToken: true);

            await _accountService.AddRefreshToken(new RefreshToken
            {
                UserId = refreshTokenEntity.UserId,
                Token = newRefreshToken
            });

            refreshTokenEntity.User.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(refreshTokenEntity.User);

            return Ok(new LoginResponseModel
            {
                Token = token,
                RefreshToken = newRefreshToken,
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid registration attempt: {@RegisterDto}", registerDto);
                    return BadRequest(ModelState);
                }

                var user = new User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email
                };

                if (string.IsNullOrEmpty(registerDto.Password))
                {
                    _logger.LogWarning("Registration failed: Password is empty for user {Email}", user.Email);
                    ModelState.AddModelError("Password", "Password is required.");
                    return BadRequest(ModelState);
                }

                var existingEmail = await _userManager.FindByEmailAsync(user.Email ?? string.Empty);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Registration failed: User already exists with email {Email}", user.Email);
                    return BadRequest("User already exists");
                }
                var existingUser = await _userManager.FindByNameAsync(user.UserName ?? string.Empty);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: User already exists with username {Username}", user.UserName);
                    return BadRequest("User already exists");
                }

                var createUser = await _userManager.CreateAsync(user, registerDto.Password);
                if (createUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");

                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation("User registered successfully: {Email}", user.Email);
                        return Ok(
                            new RegisterResponseModel
                            {
                                Username = user.UserName!,
                                Email = user.Email!,
                                Token = _tokenService.CreateToken(user, isRefreshToken: false),
                                RefreshToken = _tokenService.CreateToken(user, isRefreshToken: true),
                                ExpiresIn = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
                            }
                        );
                    }
                    else
                    {
                        _logger.LogError("Failed to assign role to user {Email}: {@Errors}", user.Email, roleResult.Errors);
                        return StatusCode(500, roleResult.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    _logger.LogError("Failed to create user {Email}: {@Errors}", user.Email, createUser.Errors);
                    return StatusCode(500, createUser.Errors.Select(e => e.Description));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception during registration for {Email}", registerDto.Email);
                return StatusCode(500, e);
            }
        }

        [HttpGet]
        [Route("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid profile request. Model state invalid.");
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Profile request failed: user not found for current identity.");
                return Unauthorized("User not found.");
            }

            _logger.LogInformation("Profile requested for user {Username}", user.UserName);

            return Ok(new UserDto
            {
                Username = user.UserName,
                CreatedAt = user.RegistrationDate,
                Email = user.Email,
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()
            });
        }

        [HttpGet("oauth/github/url")]
        public IActionResult GetGitHubOAuthUrl()
        {
            _logger.LogInformation("Generating GitHub OAuth URL");

            var clientId = _configuration["OAuth:GitHub:ClientId"];
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var redirectUri = Uri.EscapeDataString($"{frontendBaseUrl}/auth/callback/github");
            var state = Guid.NewGuid().ToString();
            var scope = "user:email";

            HttpContext.Session.SetString($"oauth_state_{state}", "github");

            var oauthUrl = $"https://github.com/login/oauth/authorize?" +
                          $"client_id={clientId}&" +
                          $"redirect_uri={redirectUri}&" +
                          $"scope={scope}&" +
                          $"state={state}";

            _logger.LogInformation("Generated GitHub OAuth URL with state: {State}", state);

            return Ok(new OAuthUrlResponse { Url = oauthUrl, State = state });
        }

        [HttpGet("oauth/google/url")]
        public IActionResult GetGoogleOAuthUrl()
        {
            _logger.LogInformation("Generating Google OAuth URL");

            var clientId = _configuration["OAuth:Google:ClientId"];
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var redirectUri = Uri.EscapeDataString($"{frontendBaseUrl}/auth/callback/google");
            var state = Guid.NewGuid().ToString();
            var scope = "openid email profile";

            HttpContext.Session.SetString($"oauth_state_{state}", "google");

            var oauthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                          $"client_id={clientId}&" +
                          $"redirect_uri={redirectUri}&" +
                          $"scope={Uri.EscapeDataString(scope)}&" +
                          $"response_type=code&" +
                          $"state={state}";

            _logger.LogInformation("Generated Google OAuth URL with state: {State}", state);

            return Ok(new OAuthUrlResponse { Url = oauthUrl, State = state });
        }

        [HttpPost("oauth/github/exchange")]
        public async Task<IActionResult> ExchangeGitHubCode([FromBody] OAuthExchangeRequest request)
        {
            _logger.LogInformation("Exchanging GitHub code for tokens");

            if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.State))
            {
                return BadRequest(new { error = "Missing code or state parameter" });
            }

            var storedProvider = HttpContext.Session.GetString($"oauth_state_{request.State}");
            if (storedProvider != "github")
            {
                _logger.LogWarning("Invalid state for GitHub OAuth: {State}", request.State);
                return BadRequest(new { error = "Invalid state parameter" });
            }

            try
            {
                var result = await ProcessGitHubOAuth(request.Code);
                if (result.IsSuccess)
                {
                    HttpContext.Session.Remove($"oauth_state_{request.State}");

                    return Ok(new LoginResponseModel
                    {
                        Token = result.Token,
                        RefreshToken = result.RefreshToken,
                        ExpiresIn = result.ExpiresIn
                    });
                }

                return BadRequest(new { error = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GitHub OAuth exchange");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("oauth/google/exchange")]
        public async Task<IActionResult> ExchangeGoogleCode([FromBody] OAuthExchangeRequest request)
        {
            _logger.LogInformation("Exchanging Google code for tokens");

            if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.State))
            {
                return BadRequest(new { error = "Missing code or state parameter" });
            }

            var storedProvider = HttpContext.Session.GetString($"oauth_state_{request.State}");
            if (storedProvider != "google")
            {
                _logger.LogWarning("Invalid state for Google OAuth: {State}", request.State);
                return BadRequest(new { error = "Invalid state parameter" });
            }

            try
            {
                var result = await ProcessGoogleOAuth(request.Code);
                if (result.IsSuccess)
                {
                    HttpContext.Session.Remove($"oauth_state_{request.State}");

                    return Ok(new LoginResponseModel
                    {
                        Token = result.Token,
                        RefreshToken = result.RefreshToken,
                        ExpiresIn = result.ExpiresIn
                    });
                }

                return BadRequest(new { error = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google OAuth exchange");
                return StatusCode(500, new { error = "Internal server error" });
            }
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
                    user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = name,
                            Email = email,
                            EmailConfirmed = true,
                            RegistrationDate = DateTime.UtcNow,
                            Provider = provider,
                            Avatar = avatar
                        };

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

                        await _userManager.AddToRoleAsync(user, "User");
                        _logger.LogInformation("Created new user from {Provider}: {Email}", provider, email);
                    }

                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        _logger.LogError("Failed to add external login for {Provider}: {@Errors}",
                            provider, addLoginResult.Errors);
                        return OAuthResult.Failure("external_login_failed");
                    }
                }

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

                await _accountService.RemoveRefreshTokenByUserId(user.Id);

                var token = _tokenService.CreateToken(user, isRefreshToken: false);
                var refreshToken = _tokenService.CreateToken(user, isRefreshToken: true);

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

        private async Task<OAuthResult> ProcessGitHubOAuth(string code)
        {
            using var httpClient = new HttpClient();

            // Exchange code for access token
            var tokenRequest = new
            {
                client_id = _configuration["OAuth:GitHub:ClientId"],
                client_secret = _configuration["OAuth:GitHub:ClientSecret"],
                code = code
            };

            var tokenContent = new StringContent(JsonSerializer.Serialize(tokenRequest), Encoding.UTF8, "application/json");
            var tokenResponse = await httpClient.PostAsync("https://github.com/login/oauth/access_token", tokenContent);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub token exchange failed: {StatusCode}", tokenResponse.StatusCode);
                return OAuthResult.Failure("token_exchange_failed");
            }

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = ParseQueryString(tokenResponseContent);

            if (!tokenData.TryGetValue("access_token", out var accessToken))
            {
                _logger.LogError("No access token in GitHub response");
                return OAuthResult.Failure("no_access_token");
            }

            // Get user info
            httpClient.DefaultRequestHeaders.Add("Authorization", $"token {accessToken}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "AnekdotifyApp");

            var userResponse = await httpClient.GetAsync("https://api.github.com/user");
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("GitHub user info request failed: {StatusCode}", userResponse.StatusCode);
                return OAuthResult.Failure("user_info_failed");
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(userJson);

            var email = userData.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var name = userData.TryGetProperty("login", out var nameProp) ? nameProp.GetString() : null;
            var externalId = userData.TryGetProperty("id", out var idProp) ? idProp.GetInt64().ToString() : null;
            var avatar = userData.TryGetProperty("avatar_url", out var avatarProp) ? avatarProp.GetString() : null;

            // If email is null, try to get it from emails endpoint
            if (string.IsNullOrEmpty(email))
            {
                var emailsResponse = await httpClient.GetAsync("https://api.github.com/user/emails");
                if (emailsResponse.IsSuccessStatusCode)
                {
                    var emailsJson = await emailsResponse.Content.ReadAsStringAsync();
                    var emailsData = JsonSerializer.Deserialize<JsonElement[]>(emailsJson);
                    email = emailsData?.FirstOrDefault(e =>
                        e.TryGetProperty("primary", out var primary) && primary.GetBoolean())
                        .GetProperty("email").GetString();
                }
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(externalId))
            {
                _logger.LogWarning("Missing email or external ID from GitHub");
                return OAuthResult.Failure("missing_user_info");
            }

            return await ProcessOAuthUser(email, name, externalId, avatar, "GitHub");
        }

        private async Task<OAuthResult> ProcessGoogleOAuth(string code)
        {
            using var httpClient = new HttpClient();

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var redirectUri = $"{frontendBaseUrl}/auth/callback/google";

            // Exchange code for access token
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _configuration["OAuth:Google:ClientId"]!),
                new KeyValuePair<string, string>("client_secret", _configuration["OAuth:Google:ClientSecret"]!),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Google token exchange failed: {StatusCode}", tokenResponse.StatusCode);
                return OAuthResult.Failure("token_exchange_failed");
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

            if (!tokenData.TryGetProperty("access_token", out var accessTokenProp))
            {
                _logger.LogError("No access token in Google response");
                return OAuthResult.Failure("no_access_token");
            }

            var accessToken = accessTokenProp.GetString();

            // Get user info
            var userResponse = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Google user info request failed: {StatusCode}", userResponse.StatusCode);
                return OAuthResult.Failure("user_info_failed");
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(userJson);

            var email = userData.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var name = userData.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
            var externalId = userData.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var avatar = userData.TryGetProperty("picture", out var avatarProp) ? avatarProp.GetString() : null;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(externalId))
            {
                _logger.LogWarning("Missing email or external ID from Google");
                return OAuthResult.Failure("missing_user_info");
            }

            return await ProcessOAuthUser(email, name, externalId, avatar, "Google");
        }

        private async Task<OAuthResult> ProcessOAuthUser(string email, string? name, string externalId, string? avatar, string provider)
        {
            try
            {
                _logger.LogInformation("Processing {Provider} login for email: {Email}, externalId: {ExternalId}",
                    provider, email, externalId);

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = name,
                        Email = email,
                        EmailConfirmed = true,
                        RegistrationDate = DateTime.UtcNow,
                        Provider = provider,
                        Avatar = avatar
                    };

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

                    await _userManager.AddToRoleAsync(user, "User");
                    _logger.LogInformation("Created new user from {Provider}: {Email}", provider, email);
                }
                else
                {
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
                }

                await _accountService.RemoveRefreshTokenByUserId(user.Id);

                var token = _tokenService.CreateToken(user, isRefreshToken: false);
                var refreshToken = _tokenService.CreateToken(user, isRefreshToken: true);

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
                _logger.LogError(ex, "Error processing OAuth user from {Provider}", provider);
                return OAuthResult.Failure("internal_error");
            }
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();
            var pairs = query.Split('&');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    result[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
                }
            }

            return result;
        }

        public class OAuthExchangeRequest
        {
            public string Code { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
        }

        public class OAuthUrlResponse
        {
            public string Url { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
        }
    }
}