using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public AccountController(
            UserManager<User> userManager,
            ITokenService tokenService,
            SignInManager<User> signInManager,
            IAccountService accountService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _accountService = accountService;
            _logger = logger;
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

                var existing = await _userManager.FindByEmailAsync(user.Email ?? string.Empty);
                if (existing != null)
                {
                    _logger.LogWarning("Registration failed: User already exists with email {Email}", user.Email);
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
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    _logger.LogError("Failed to create user {Email}: {@Errors}", user.Email, createUser.Errors);
                    return StatusCode(500, createUser.Errors);
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
    }
}