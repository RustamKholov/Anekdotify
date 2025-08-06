using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Database.Data;
using Anekdotify.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Anekdotify.BL.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TokenService> _logger;
        private readonly ApplicationDBContext _context;

        public TokenService(IConfiguration config, UserManager<User> userManager, ILogger<TokenService> logger,
            ApplicationDBContext context)
        {
            _config = config;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public string CreateToken(User user, bool isRefreshToken = false)
        {
            if (user == null)
            {
                _logger.LogError("Attempted to create token for null user.");
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            var userRoles = _userManager.GetRolesAsync(user).Result;

            if (userRoles == null || !userRoles.Any())
            {
                _logger.LogError("User roles cannot be null or empty for user {UserId}", user.Id);
                throw new InvalidOperationException("User roles cannot be null or empty");
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingKey = isRefreshToken ? _config["JWT:RefreshSigningKey"] : _config["JWT:SigningKey"];

            if (string.IsNullOrEmpty(signingKey))
            {
                _logger.LogCritical("JWT signing key is not configured. isRefreshToken={IsRefreshToken}", isRefreshToken);
                throw new InvalidOperationException("JWT signing key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(isRefreshToken ? 24 * 60 : 30),
                SigningCredentials = credentials,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            _logger.LogInformation("Created {TokenType} token for user {UserId} (expires: {Expires})",
                isRefreshToken ? "refresh" : "access", user.Id, tokenDescriptor.Expires);

            return tokenHandler.WriteToken(token);
        }
        public async Task<string> CreateRefreshTokenAsync(User user)
        {
            var refreshToken = CreateToken(user, isRefreshToken: true);

            // Store refresh token in database
            var tokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token, string userId)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && rt.UserId == userId && !rt.IsRevoked);

            return refreshToken != null && refreshToken.ExpiryDate > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

    }
}