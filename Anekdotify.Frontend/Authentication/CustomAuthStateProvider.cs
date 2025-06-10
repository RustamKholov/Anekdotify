using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Anekdotify.Frontend.Heplers;
using Microsoft.AspNetCore.Components.Authorization;

namespace Anekdotify.Frontend.Authentication;

public class CustomAuthStateProvider(ProtectedBrowserStorageAccessor protectedLocalStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await protectedLocalStorage.GetLocalStorageAsync<string>("authToken");

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
        await protectedLocalStorage.SetLocalStorageAsync("authToken", token);
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
