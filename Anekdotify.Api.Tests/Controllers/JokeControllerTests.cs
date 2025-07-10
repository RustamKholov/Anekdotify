using System.Security.Claims;
using Anekdotify.Api.Controllers;
using Anekdotify.Api.Tests.TestBase;
using Anekdotify.BL.Interfaces.Services;
using Anekdotify.Common;
using Anekdotify.Models.DTOs.Jokes;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Anekdotify.Api.Tests.Controllers
{
    public class JokeControllerTests : ControllerTestBase<JokeController, JokeController>
    {
        private readonly Mock<IJokeService> _jokeServiceMock;
        private readonly Mock<IUserSavedJokeService> _userSavedJokeServiceMock;
        private readonly Mock<IUserViewedJokesService> _userViewedJokesServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;

        public JokeControllerTests()
        {
            _jokeServiceMock = new Mock<IJokeService>();
            _userSavedJokeServiceMock = new Mock<IUserSavedJokeService>();
            _userViewedJokesServiceMock = new Mock<IUserViewedJokesService>();
            
            // Mock UserManager
            var storeMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                storeMock.Object, null, null, null, null, null, null, null, null);
            
            Controller = new JokeController(
                _jokeServiceMock.Object,
                _userSavedJokeServiceMock.Object,
                _userViewedJokesServiceMock.Object,
                _userManagerMock.Object,
                LoggerMock.Object);
            
            // Set up the controller context with a default user
            SetupControllerContext();
        }
        
        [Fact]
        public async Task GetJokeById_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var jokeId = 1;
            var jokeDto = new JokeDto { JokeId = jokeId, Text = "Test joke", ClassificationName = "Funny" };
            
            _jokeServiceMock.Setup(s => s.GetJokeByIdAsync(jokeId))
                .ReturnsAsync(jokeDto);
            
            // Act
            var result = await Controller.GetJokeById(jokeId);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedJoke = Assert.IsType<JokeDto>(okResult.Value);
            Assert.Equal(jokeId, returnedJoke.JokeId);
            Assert.Equal(jokeDto.Text, returnedJoke.Text);
        }
        
        [Fact]
        public async Task GetJokeById_NonexistentId_ReturnsNotFound()
        {
            // Arrange
            var jokeId = 999;
            _jokeServiceMock.Setup(s => s.GetJokeByIdAsync(jokeId))
                .ReturnsAsync((JokeDto)null);
            
            // Act
            var result = await Controller.GetJokeById(jokeId);
            
            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        
        [Fact]
        public async Task GetRandomJoke_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var userId = "testuser-123";
            var user = new User 
            { 
                Id = userId,
                LastJokeRetrievalDate = DateTime.UtcNow.AddDays(-1) // More than 24 hours ago
            };
            
            var jokeDto = new JokeDto { JokeId = 1, Text = "Random joke" };
            
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            _userViewedJokesServiceMock.Setup(s => s.GetViewedJokesAsync(userId))
                .ReturnsAsync(OperationResult<List<int>>.Success(new List<int>()));
                
            _jokeServiceMock.Setup(s => s.GetRandomJokeAsync(It.IsAny<List<int>>(), null))
                .ReturnsAsync(jokeDto);
                
            // Act
            var result = await Controller.GetRandomJokeAsync();
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedJoke = Assert.IsType<JokeDto>(okResult.Value);
            Assert.Equal(jokeDto.JokeId, returnedJoke.JokeId);
            Assert.Equal(jokeDto.Text, returnedJoke.Text);
            
            // Verify user viewed joke was added
            _userViewedJokesServiceMock.Verify(s => s.AddViewedJokeAsync(userId, jokeDto.JokeId), Times.Once);
            
            // Verify user's LastJokeRetrievalDate was updated
            _userManagerMock.Verify(m => m.UpdateAsync(It.Is<User>(u => u.Id == userId)), Times.Once);
        }
        
        [Fact]
        public async Task GetRandomJoke_JokeAlreadyRetrievedWithin24Hours_ReturnsBadRequest()
        {
            // Arrange
            var userId = "testuser-123";
            var user = new User 
            { 
                Id = userId,
                LastJokeRetrievalDate = DateTime.UtcNow.AddHours(-12) // Less than 24 hours ago
            };
            
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
                
            // Admin or Moderator check will return false
            SetupControllerContext(userId, "testuser", new[] { "User" });
            
            // Act
            var result = await Controller.GetRandomJokeAsync();
            
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        
        /* [Fact]
        public async Task SuggestJoke_ValidJoke_ReturnsOkResult()
        {
            // Arrange
            var jokeCreateDto = new JokeCreateDto
            {
                Text = "Why did the chicken cross the road?",
                ClassificationId = 1
            };
            
            var createdJoke = new JokeDto
            {
                JokeId = 1,
                Text = jokeCreateDto.Text,
                ClassificationName = "Funny",
                IsApproved = false
            };
            
            _jokeServiceMock.Setup(s => s.SuggestJokeAsync(
                    It.Is<JokeCreateDto>(j => j.Text == jokeCreateDto.Text), 
                    It.IsAny<string>()))
                .ReturnsAsync(createdJoke);
            
            // Act
            var result = await Controller.SuggestJokeAsync(jokeCreateDto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedJoke = Assert.IsType<JokeDto>(okResult.Value);
            Assert.Equal(createdJoke.JokeId, returnedJoke.JokeId);
            Assert.Equal(createdJoke.Text, returnedJoke.Text);
            Assert.False(returnedJoke.IsApproved); // Suggested jokes should not be approved by default
        }  */
    }
}