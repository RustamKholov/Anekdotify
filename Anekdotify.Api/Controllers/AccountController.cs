
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Anekdotify.BL.Interfaces;
using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Anekdotify.Api.Controllers
{
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IAccountService _accountService;
        public AccountController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager, IConfiguration config, IAccountService accountService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _config = config;
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(loginDTO);
            }
            // Accept camelCase by normalizing property names
            User? user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginDTO.Username.ToLower() || u.Email == loginDTO.Username.ToLower());

            if (user == null) return Unauthorized("Username not found and/or wrong password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded) return Unauthorized("Username not found and/or wrong password");

            var token = _tokenService.CreateToken(user, isRefreshToken:false);
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
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() // 1 hour expiration
            };
            return Ok(loginResponse);
        }
        [HttpGet("loginByRefreshToken")]
        public async Task<ActionResult<LoginResponseModel>> LoginByRefreshToken([FromQuery] string refreshToken)
        {
            var refreshTokenEntity = await _accountService.GetRefreshTokenByToken(refreshToken);
            if (refreshTokenEntity == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            var token = _tokenService.CreateToken(refreshTokenEntity.User, isRefreshToken: false);
            var newRefreshToken = _tokenService.CreateToken(refreshTokenEntity.User, isRefreshToken: true);

            await _accountService.AddRefreshToken(new RefreshToken
            {
                UserId = refreshTokenEntity.UserId,
                Token = newRefreshToken
            });

            return Ok(new LoginResponseModel
            {
                Token = token,
                RefreshToken = newRefreshToken,
                ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() // 1 hour expiration
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = new User
                {
                    UserName = registerDTO.Username,
                    Email = registerDTO.Email
                };

                if (string.IsNullOrEmpty(registerDTO.Password))
                {
                    ModelState.AddModelError("Password", "Password is required.");
                    return BadRequest(ModelState);
                }

                var createUser = await _userManager.CreateAsync(user, registerDTO.Password);
                if (createUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        return Ok(
                            new NewUserDTO
                            {
                                UserName = user.UserName!,
                                Email = user.Email!,
                                Token = _tokenService.CreateToken(user, isRefreshToken: false),
                                RefreshToken = _tokenService.CreateToken(user, isRefreshToken: true),
                                ExpiresIn = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() // 1 hour expiration
                            }
                        );
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, createUser.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

    }
}