using TaskTrackingSystem.Shared.Models.Menu;

namespace TaskTrackingSystem.WebApp;

public class UserSessionState
{
    public string? Token { get; set; }

    // Cached per-role menus — shared between NavMenu and MainLayout
    public string? CachedMenuRole { get; set; }
    public List<MenuDto>? CachedMenus { get; set; }

    public void ClearMenuCache()
    {
        CachedMenuRole = null;
        CachedMenus = null;
    }

    public void ClearSession()
    {
        Token = null;
        ClearMenuCache();
    }
}
