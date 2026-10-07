using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using WebClient.Shared.Features.Learning;

namespace WebClient.Shared.Features.StaticDemo;

/// <summary>
/// Client-only demo personas for the static WebAssembly demo. Anything running in the browser can be changed
/// by the user, so this drives the UI only; it is not authorization. The server host enforces policies.
/// </summary>
public sealed class DemoAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _user = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(_user));

    public void SignIn(string persona)
    {
        if (!LearningPolicies.Personas.Contains(persona)) throw new ArgumentOutOfRangeException(nameof(persona));
        _user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, $"demo:{persona}"),
            new Claim(ClaimTypes.Name, $"Demo {persona}"),
            new Claim(ClaimTypes.Role, persona)
        ], authenticationType: "BrowserDemo"));
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
