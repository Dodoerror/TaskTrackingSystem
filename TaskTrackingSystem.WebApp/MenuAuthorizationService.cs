using System.Net.Http.Json;
using System.Security.Claims;
using TaskTrackingSystem.Shared.Models.Menu;

namespace TaskTrackingSystem.WebApp;

/// <summary>
/// Centralizes menu loading and route authorization checks for the Blazor UI.
/// </summary>
public class MenuAuthorizationService
{
    private static readonly HashSet<string> PublicRouteSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        string.Empty,
        "login",
        "register",
        "error"
    };

    private readonly ApiClientService _apiClient;
    private readonly UserSessionState _sessionState;

    public MenuAuthorizationService(ApiClientService apiClient, UserSessionState sessionState)
    {
        _apiClient = apiClient;
        _sessionState = sessionState;
    }

    public static bool IsPublicRoute(string relativePath)
    {
        var firstSegment = GetFirstSegment(relativePath);
        return PublicRouteSegments.Contains(firstSegment);
    }

    public static string GetFirstSegment(string relativePath)
    {
        var path = relativePath.Split('?')[0].Trim('/');
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return path.Split('/')[0];
    }

    public static string NormalizeMenuHref(string? menuUrl)
    {
        if (string.IsNullOrWhiteSpace(menuUrl))
        {
            return "/dashboard";
        }

        return menuUrl.StartsWith('/') ? menuUrl : $"/{menuUrl.TrimStart('/')}";
    }

    public async Task<List<MenuDto>> GetUserMenusAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return new List<MenuDto>();
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

        if (_sessionState.CachedMenuRole == role && _sessionState.CachedMenus != null)
        {
            return _sessionState.CachedMenus;
        }

        var client = _apiClient.CreateClient(user);
        var response = await client.GetAsync("Menu");
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Menu API failed: {(int)response.StatusCode} {response.ReasonPhrase}");
            return new List<MenuDto>();
        }

        var menus = await response.Content.ReadFromJsonAsync<List<MenuDto>>() ?? new List<MenuDto>();

        _sessionState.CachedMenus = menus;
        _sessionState.CachedMenuRole = role;

        return menus;
    }

    public bool IsRouteAllowed(IReadOnlyList<MenuDto> menus, string relativePath)
    {
        var firstSegment = GetFirstSegment(relativePath);

        if (PublicRouteSegments.Contains(firstSegment))
        {
            return true;
        }

        if (firstSegment.Equals("dashboard", StringComparison.OrdinalIgnoreCase) ||
            firstSegment.Equals("home", StringComparison.OrdinalIgnoreCase))
        {
            return MenuCollectionMatchesSegment(menus, "dashboard");
        }

        return MenuCollectionMatchesSegment(menus, firstSegment);
    }

    private static bool MenuCollectionMatchesSegment(IReadOnlyList<MenuDto> menus, string segment)
    {
        foreach (var menu in menus)
        {
            if (MenuMatchesSegment(menu, segment))
            {
                return true;
            }

            foreach (var subMenu in menu.SubMenus)
            {
                if (MenuMatchesSegment(subMenu, segment))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool MenuMatchesSegment(MenuDto menu, string segment)
    {
        if (string.IsNullOrWhiteSpace(menu.MenuUrl))
        {
            return false;
        }

        var menuSegment = menu.MenuUrl.Split('?')[0].Trim('/').Split('/')[0];
        return menuSegment.Equals(segment, StringComparison.OrdinalIgnoreCase);
    }
}
