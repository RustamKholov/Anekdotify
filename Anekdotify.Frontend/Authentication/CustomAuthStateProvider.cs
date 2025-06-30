using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Anekdotify.Models.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Anekdotify.Frontend.Authentication;

public class CustomAuthStateProvider(ProtectedLocalStorage protectedLocalStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var sessionModel = (await protectedLocalStorage.GetAsync<LoginResponseModel>("sessionState")).Value;

        ClaimsIdentity identity;
        if (sessionModel != null && !string.IsNullOrEmpty(sessionModel.Token))
        {
            identity = GetClaimsIdentity(sessionModel.Token);
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
    public async Task MarkUserAsAuthenticated(LoginResponseModel loginResponse)
    {
        await protectedLocalStorage.SetAsync("sessionState", loginResponse);
        ClaimsIdentity identity;
        if (!string.IsNullOrEmpty(loginResponse.Token))
        {
            identity = GetClaimsIdentity(loginResponse.Token);
        }
        else
        {
            identity = new ClaimsIdentity();
        }
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
    private ClaimsIdentity GetClaimsIdentity(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var claims = jwtToken.Claims.Select(c =>
        {
            if (c.Type == "role")
                return new Claim(ClaimTypes.Role, c.Value);
            return c;
        }).ToList();
        return new ClaimsIdentity(claims, "jwt");
    }

    public async Task MarkUserAsLoggedOut()
    {
        await protectedLocalStorage.DeleteAsync("sessionState");
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
