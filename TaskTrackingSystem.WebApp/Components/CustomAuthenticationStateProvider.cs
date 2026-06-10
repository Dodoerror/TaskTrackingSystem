using Microsoft.AspNetCore.Components.Authorization;

namespace TaskTrackingSystem.WebApp.Components;

// Cookie-only auth: reads the signed-in user from HttpContext (set by /account/login).
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly AuthenticationState Anonymous =
        new(new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity()));

    public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(new AuthenticationState(user));
        }

        return Task.FromResult(Anonymous);
    }
}
