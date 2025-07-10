using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Anekdotify.Api.Tests.TestBase
{
    public abstract class ControllerTestBase<TController, TLogger> where TController : ControllerBase
                                                                  where TLogger : class
    {
        protected readonly Mock<ILogger<TLogger>> LoggerMock;
        protected TController Controller;

        protected ControllerTestBase()
        {
            LoggerMock = new Mock<ILogger<TLogger>>();
        }

        protected void SetupControllerContext(string userId = "testuser-123", string username = "testuser", string[]? roles = null)
        {
            roles ??= new[] { "User" };

            // Create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create ClaimsPrincipal
            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            var principal = new ClaimsPrincipal(identity);

            // Set up HttpContext
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            // Set ControllerContext
            Controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}