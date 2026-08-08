using Microsoft.AspNetCore.Authentication.Cookies;
using BookLoverECommerce.Cms.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder(
        "/",
        "AdminOnly");

    options.Conventions.AllowAnonymousToPage(
        "/Account/Login");

    options.Conventions.AllowAnonymousToPage(
        "/Account/AccessDenied");

    options.Conventions.AllowAnonymousToPage(
        "/Error");
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".BookLoverECommerce.Cms.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name =
            ".BookLoverECommerce.Cms.Authentication";
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "AdminOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
});

// calling the backend service "ApiGateway"
var apiGatewayBaseUrl =
    builder.Configuration["ApiGateway:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiGateway:BaseUrl is not configured.");

builder.Services
    .AddHttpClient("ApiGateway", client =>
    {
        client.BaseAddress = new Uri(
            apiGatewayBaseUrl.TrimEnd('/') + "/");

        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<GatewayAuthHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<GatewayAuthHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();