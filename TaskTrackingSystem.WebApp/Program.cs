using Microsoft.AspNetCore.Authentication.Cookies;
using TaskTrackingSystem.WebApp;
using TaskTrackingSystem.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = true);

// Register HttpClient for WebApi calls
var webApiBaseUrl = builder.Configuration["WebApi:BaseUrl"] ?? "https://localhost:7215/api/";

var webApiBuilder = builder.Services.AddHttpClient("WebApi", client =>
{
    client.BaseAddress = new Uri(webApiBaseUrl);
});

if (builder.Environment.IsDevelopment() &&
    webApiBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
{
    webApiBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<ApiClientService>();
builder.Services.AddScoped<MenuAuthorizationService>();

// Cookie authentication for Blazor pages and HTTP middleware.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, CustomAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapAccountEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
