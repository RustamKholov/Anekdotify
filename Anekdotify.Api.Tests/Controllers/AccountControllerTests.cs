using Anekdotify.Api.Controllers;
using Anekdotify.Api.Tests.TestBase;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Anekdotify.Api.Tests.Controllers
{
    public class AccountControllerTests : ControllerTestBase<AccountController, AccountController>
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public AccountControllerTests()
        {
            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Mock SignInManager
            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null!, null!, null!, null!);

            _tokenServiceMock = new Mock<ITokenService>();
            _accountServiceMock = new Mock<IAccountService>();
            _configurationMock = new Mock<IConfiguration>();

            // Setup configuration mock
            _configurationMock.Setup(x => x["Frontend:BaseUrl"]).Returns("http://localhost:5173");

            Controller = new AccountController(
                _userManagerMock.Object,
                _tokenServiceMock.Object,
                _signInManagerMock.Object,
                _accountServiceMock.Object,
                LoggerMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(m => m.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null); // User doesn't exist yet

            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<User>(), false))
                .Returns("test-jwt-token");

            _tokenServiceMock.Setup(t => t.CreateToken(It.IsAny<User>(), true))
                .Returns("test-refresh-token");

            // Act
            var result = await Controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponseModel>(okResult.Value);
            Assert.Equal(registerDto.Username, response.Username);
            Assert.Equal(registerDto.Email, response.Email);
            Assert.Equal("test-jwt-token", response.Token);
            Assert.Equal("test-refresh-token", response.RefreshToken);
        }

        [Fact]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Email = "existing@test.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(m => m.FindByEmailAsync(registerDto.Email))
                .ReturnsAsync(new User { Email = registerDto.Email }); // User already exists

            // Act
            var result = await Controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var user = new User
            {
                UserName = loginDto.Username,
                Email = "testuser@test.com"
            };

            _userManagerMock.Setup(m => m.FindByNameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _tokenServiceMock.Setup(t => t.CreateToken(user, false))
                .Returns("test-jwt-token");

            _tokenServiceMock.Setup(t => t.CreateToken(user, true))
                .Returns("test-refresh-token");

            // Act
            var result = await Controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var loginResponse = Assert.IsType<LoginResponseModel>(okResult.Value);
            Assert.Equal("test-jwt-token", loginResponse.Token);
            Assert.Equal("test-refresh-token", loginResponse.RefreshToken);

            // Verify refresh token was saved
            _accountServiceMock.Verify(a => a.AddRefreshToken(It.Is<RefreshToken>(
                rt => rt.UserId == user.Id && rt.Token == "test-refresh-token")), Times.Once);

            // Verify last login date was updated
            _userManagerMock.Verify(m => m.UpdateAsync(It.Is<User>(u =>
                u.UserName == loginDto.Username && u.LastLoginDate != null)), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "wronguser",
                Password = "WrongPassword"
            };

            var user = new User
            {
                UserName = loginDto.Username,
                Email = "wronguser@test.com"
            };

            _userManagerMock.Setup(m => m.FindByNameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await Controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}