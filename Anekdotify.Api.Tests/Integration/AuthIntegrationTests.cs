using Anekdotify.Api.Tests.TestBase;
using Anekdotify.Models.DTOs.Accounts;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Anekdotify.Models.Models;
using Xunit;

namespace Anekdotify.Api.Tests.Integration
{
    public class AuthIntegrationTests : IntegrationTestBase
    {
        public AuthIntegrationTests(AnekdotifyApiFactory factory) : base(factory)
        {
        }
        
        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = $"newuser{Guid.NewGuid():N}",
                Email = $"newuser{Guid.NewGuid():N}@test.com",
                Password = "Password123!"
            };
            
            var content = new StringContent(
                JsonConvert.SerializeObject(registerDto),
                Encoding.UTF8,
                "application/json");
            
            // Act
            var response = await Client.PostAsync("/api/account/register", content);
            
            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RegisterResponseModel>(responseString);
            
            Assert.NotNull(result);
            Assert.Equal(registerDto.Username, result.Username);
            Assert.Equal(registerDto.Email, result.Email);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }
        
        [Fact]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "user@test.com", // Already exists from the seed data
                Email = "user@test.com",
                Password = "Password123!"
            };
            
            var content = new StringContent(
                JsonConvert.SerializeObject(registerDto),
                Encoding.UTF8,
                "application/json");
            
            // Act
            var response = await Client.PostAsync("/api/account/register", content);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "user@test.com",
                Password = "User123!"
            };
            
            var content = new StringContent(
                JsonConvert.SerializeObject(loginDto),
                Encoding.UTF8,
                "application/json");
            
            // Act
            var response = await Client.PostAsync("/api/account/login", content);
            
            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponseModel>(responseString);
            
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }
        
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "user@test.com",
                Password = "WrongPassword!"
            };
            
            var content = new StringContent(
                JsonConvert.SerializeObject(loginDto),
                Encoding.UTF8,
                "application/json");
            
            // Act
            var response = await Client.PostAsync("/api/account/login", content);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task GetProfile_Authenticated_ReturnsProfile()
        {
            // Arrange
            await AuthenticateAsync();
            
            // Act
            var response = await Client.GetAsync("/api/account/profile");
            
            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var userDto = JsonConvert.DeserializeObject<UserDto>(responseString);
            
            Assert.NotNull(userDto);
            Assert.Equal("user@test.com", userDto.Username);
            Assert.Equal("user@test.com", userDto.Email);
            Assert.Equal("User", userDto.Role);
        }
        
        [Fact]
        public async Task GetProfile_Unauthenticated_ReturnsUnauthorized()
        {
            // Arrange - No authentication
            Client.DefaultRequestHeaders.Authorization = null;
            
            // Act
            var response = await Client.GetAsync("/api/account/profile");
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}