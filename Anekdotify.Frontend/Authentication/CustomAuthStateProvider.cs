using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Anekdotify.Frontend.Heplers;
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
        if (sessionModel != null)
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
        var identity = GetClaimsIdentity(loginResponse.Token);
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

    public async Task MarkUserAsLoggedOut()
    {
        await protectedLocalStorage.DeleteAsync("sessionState");
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
