using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TaskTrackingSystem.WebApp.Components;

// Cookie-only auth: reads the signed-in user from HttpContext (set by /account/login).
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserSessionState _sessionState;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor, UserSessionState sessionState)
    {
        _httpContextAccessor = httpContextAccessor;
        _sessionState = sessionState;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var token = user.FindFirst("jwt_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _sessionState.Token = token;
            }
            return Task.FromResult(new AuthenticationState(user));
        }

        return Task.FromResult(Anonymous);
    }

    public void NotifyUserAuthenticationChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
