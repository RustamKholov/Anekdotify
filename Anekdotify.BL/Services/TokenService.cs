using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Anekdotify.BL.Interfaces;
using Anekdotify.Models.Entities;   
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Anekdotify.BL.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _config = config;
            var signingKey = _config["JWT:SigningKey"];
            if (string.IsNullOrEmpty(signingKey))
            {
                throw new InvalidOperationException("JWT signing key is not configured.");
            }
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = credentials,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}