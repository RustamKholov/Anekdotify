using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Anekdotify.Database.Data;
using Anekdotify.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Text;
using Anekdotify.Models.DTOs.Accounts;
using Anekdotify.Models.Models;
using Newtonsoft.Json;
using Anekdotify.Api.Tests.TestBase;

namespace Anekdotify.Api.Tests.TestBase
{
    public class AnekdotifyApiFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDBContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing
                services.AddDbContext<ApplicationDBContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryAnekdotifyTest");
                });

                // Configure identity to use the same in-memory database
                services.Configure<IdentityOptions>(options =>
                {
                    // Make password requirements easier for testing
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDBContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<User>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed the database with test data
                SeedTestData(db, userManager, roleManager).Wait();
            });

            return base.CreateHost(builder);
        }

        private async Task SeedTestData(ApplicationDBContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Moderator"))
                await roleManager.CreateAsync(new IdentityRole("Moderator"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // Create admin user
            var adminUser = new User
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create regular user
            var regularUser = new User
            {
                UserName = "user@test.com",
                Email = "user@test.com",
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };

            if (await userManager.FindByEmailAsync(regularUser.Email) == null)
            {
                await userManager.CreateAsync(regularUser, "User123!");
                await userManager.AddToRoleAsync(regularUser, "User");
            }

            // Add classifications if they don't exist
            if (!context.Classifications.Any())
            {
                context.Classifications.AddRange(
                    new Classification { ClassificationId = 1, Name = "Funny" },
                    new Classification { ClassificationId = 2, Name = "Dad Joke" },
                    new Classification { ClassificationId = 3, Name = "Dark Humor" },
                    new Classification { ClassificationId = 4, Name = "Pun" }
                );
                await context.SaveChangesAsync();
            }

            // Add specific test jokes with expected IDs
            if (!context.Jokes.Any())
            {
                var newRegularUser = await userManager.FindByEmailAsync("user@test.com");
                if (newRegularUser == null)
                {
                    throw new InvalidOperationException("Test user 'user@test.com' was not found.");
                }
                var regularUserId = newRegularUser.Id;

                context.Jokes.AddRange(
                    new Joke
                    {
                        JokeId = 1, // Make sure this ID matches what your tests expect
                        Text = "Why don't scientists trust atoms? Because they make up everything!",
                        ClassificationId = 1,
                        IsApproved = true,
                        SubbmissionDate = DateTime.UtcNow.AddDays(-5),
                        SubbmitedByUserId = regularUserId
                    },
                    new Joke
                    {
                        JokeId = 2,
                        Text = "How do you organize a space party? You planet!",
                        ClassificationId = 1,
                        IsApproved = true,
                        SubbmissionDate = DateTime.UtcNow.AddDays(-3),
                        SubbmitedByUserId = regularUserId
                    },
                    // Add any other specific jokes that your tests expect
                    new Joke
                    {
                        JokeId = 100, // If a specific test is looking for ID 100
                        Text = "Test joke with ID 100",
                        ClassificationId = 2,
                        IsApproved = true,
                        SubbmissionDate = DateTime.UtcNow.AddDays(-1),
                        SubbmitedByUserId = regularUserId
                    }
                );
                await context.SaveChangesAsync();
            }
            await context.SaveChangesAsync();
        }
    }
}

public abstract class IntegrationTestBase : IClassFixture<AnekdotifyApiFactory>
{
    protected readonly HttpClient Client;
    protected readonly AnekdotifyApiFactory Factory;

    protected IntegrationTestBase(AnekdotifyApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task AuthenticateAsync(string username = "user@test.com", string password = "User123!")
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var loginDto = new LoginDto { Username = username, Password = password };
        var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/account/login", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonConvert.DeserializeObject<LoginResponseModel>(responseContent);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.Token);
    }

    protected async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin@test.com", "Admin123!");
    }
}