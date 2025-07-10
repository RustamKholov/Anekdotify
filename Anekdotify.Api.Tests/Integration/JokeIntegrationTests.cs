using Anekdotify.Api.Tests.TestBase;
using Anekdotify.Models.DTOs.Jokes;
using Newtonsoft.Json;
using System.Net;
using Anekdotify.Models.Entities;
using Anekdotify.Models.Models;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Anekdotify.Database.Data;

namespace Anekdotify.Api.Tests.Integration
{
    public class JokeIntegrationTests : IntegrationTestBase
    {
        public JokeIntegrationTests(AnekdotifyApiFactory factory) : base(factory)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetJokeById_ExistingJoke_ReturnsJoke()
        {
            // Arrange
            await AuthenticateAsync();
            var id = 1; // From seed data

            // Act
            var response = await Client.GetAsync($"/api/joke/{id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var joke = JsonConvert.DeserializeObject<JokeDto>(responseString);

            Assert.NotNull(joke);
            Assert.Equal(id, joke.JokeId);
            Assert.NotEmpty(joke.Text);
        }

        [Fact]
        public async Task GetJokeById_NonExistentJoke_ReturnsNotFound()
        {
            // Arrange
            await AuthenticateAsync();
            var jokeId = 999; // Non-existent ID

            // Act
            var response = await Client.GetAsync($"/api/joke/{jokeId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SuggestJoke_ValidJoke_ReturnsNewJoke()
        {
            // Arrange
            await AuthenticateAsync();

            var jokeCreateDto = new JokeCreateDto
            {
                Text = $"Test joke {Guid.NewGuid()}",
                ClassificationId = 1 // From seed data
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(jokeCreateDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/joke/suggest", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var joke = JsonConvert.DeserializeObject<JokeDto>(responseString);

            Assert.NotNull(joke);
            Assert.Equal(jokeCreateDto.Text, joke.Text);
            Assert.False(joke.IsApproved == true); // Suggested jokes should not be approved by default
        }

        [Fact]
        public async Task GetAllJokes_Authenticated_ReturnsJokeList()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await Client.GetAsync("/api/joke");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var jokes = JsonConvert.DeserializeObject<List<JokeDto>>(responseString);

            Assert.NotNull(jokes);
            Assert.NotEmpty(jokes);
        }

        [Fact]
        public async Task IsRandomJokeActive_Authenticated_ReturnsStatus()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await Client.GetAsync("/api/joke/random/isActive");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IsActiveRandomResponse>(responseString);

            Assert.NotNull(result);
            // We can't reliably test the exact value, as it depends on the user's last retrieval date
            Assert.IsType<bool>(result.IsActive);
        }

        [Fact]
        public async Task CreateJoke_AsAdmin_CreatesJoke()
        {
            // Arrange
            await AuthenticateAsAdminAsync();

            var jokeCreateDto = new JokeCreateDto
            {
                Text = $"Admin joke {Guid.NewGuid()}",
                ClassificationId = 1, // From seed data
                SourceId = 1
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(jokeCreateDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/joke", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var joke = JsonConvert.DeserializeObject<Joke>(responseString);

            Assert.NotNull(joke);
            Assert.Equal(jokeCreateDto.Text, joke.Text);
        }

        [Fact]
        public async Task CreateJoke_AsRegularUser_ReturnsForbidden()
        {
            // Arrange
            await AuthenticateAsync(); // Regular user

            var jokeCreateDto = new JokeCreateDto
            {
                Text = $"User joke {Guid.NewGuid()}",
                ClassificationId = 1, // From seed data
                SourceId = 1
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(jokeCreateDto),
                System.Text.Encoding.UTF8,
                "application/json");

            // Act
            var response = await Client.PostAsync("/api/joke", content);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}