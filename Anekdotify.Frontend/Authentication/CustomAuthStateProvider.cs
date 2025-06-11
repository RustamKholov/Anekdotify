using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Anekdotify.Frontend.Heplers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Anekdotify.Frontend.Authentication;

public class CustomAuthStateProvider(ProtectedLocalStorage protectedLocalStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = (await protectedLocalStorage.GetAsync<string>("authToken")).Value;

        ClaimsIdentity identity;
        if (!string.IsNullOrEmpty(token))
        {
            identity = GetClaimsIdentity(token);
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
    public async Task MarkUserAsAuthenticated(string token)
    {
        await protectedLocalStorage.SetAsync("authToken", token);
        var identity = GetClaimsIdentity(token);
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
    private ClaimsIdentity GetClaimsIdentity(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var claims = jwtToken.Claims;
        return new ClaimsIdentity(claims, "jwt");
    }
}
