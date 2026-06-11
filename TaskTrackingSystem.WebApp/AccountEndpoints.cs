using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.Auth;
using TaskTrackingSystem.WebApp.Components.Partial;

namespace TaskTrackingSystem.WebApp;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", LoginAsync);
        app.MapPost("/account/register", RegisterAsync);
        app.MapGet("/account/logout", (Delegate)LogoutAsync);
        app.MapPost("/account/logout", (Delegate)LogoutAsync);
    }

    private static async Task<IResult> LoginAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        [FromForm] string usernameOrEmail,
        [FromForm] string password,
        [FromForm] bool? rememberMe,
        [FromForm] string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return RedirectToLogin(returnUrl, "invalid");
        }

        var client = httpClientFactory.CreateClient("WebApi");
        var response = await client.PostAsJsonAsync("Auth/login", new LoginDto
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        });

        if (!response.IsSuccessStatusCode)
        {
            return RedirectToLogin(returnUrl, "invalid");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponseDto>>(Serialization.CaseInsensitive);
        if (result?.IsSuccess != true || result.Value == null || string.IsNullOrWhiteSpace(result.Value.Username))
        {
            return RedirectToLogin(returnUrl, "invalid");
        }

        await SignInUserAsync(context, result.Value, rememberMe ?? false);
        return Results.Redirect(GetSafeReturnUrl(returnUrl));
    }

    private static async Task<IResult> RegisterAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        [FromForm] RegisterDto registerDto)
    {
        var client = httpClientFactory.CreateClient("WebApi");
        var response = await client.PostAsJsonAsync("Auth/register", registerDto);

        if (!response.IsSuccessStatusCode)
        {
            return Results.Redirect("/register?error=failed");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponseDto>>(Serialization.CaseInsensitive);
        if (result?.IsSuccess != true || result.Value == null || string.IsNullOrWhiteSpace(result.Value.Username))
        {
            return Results.Redirect("/register?error=failed");
        }

        await SignInUserAsync(context, result.Value, false);
        return Results.Redirect("/dashboard");
    }

    private static async Task<IResult> LogoutAsync(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login?loggedOut=true");
    }

    private static async Task SignInUserAsync(HttpContext context, AuthResponseDto authResult, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, authResult.Username),
            new(ClaimTypes.Email, authResult.Email)
        };

        if (!string.IsNullOrWhiteSpace(authResult.RoleName))
        {
            claims.Add(new Claim(ClaimTypes.Role, authResult.RoleName));
        }

        var userId = TryGetUserIdFromJwt(authResult.Token);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        claims.Add(new Claim("jwt_token", authResult.Token));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            });
    }

    private static string? TryGetUserIdFromJwt(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var payload = parts[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var json = Convert.FromBase64String(payload);
            var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (data == null)
            {
                return null;
            }

            if (data.TryGetValue("nameid", out var nameId))
            {
                return nameId.GetString();
            }

            if (data.TryGetValue(ClaimTypes.NameIdentifier, out var fullNameId))
            {
                return fullNameId.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static IResult RedirectToLogin(string? returnUrl, string error)
    {
        var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
            ? string.Empty
            : $"&ReturnUrl={Uri.EscapeDataString(returnUrl)}";

        return Results.Redirect($"/login?error={error}{safeReturnUrl}");
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//"))
        {
            return "/dashboard";
        }

        return returnUrl;
    }
}
