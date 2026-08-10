using BookLoverECommerce.Web.Services.ApiClients;
using BookLoverECommerce.Web.Services.Handlers;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var gatewayUrl =
    builder.Configuration["ApiGateway:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiGateway:BaseUrl is not configured.");


// =========================================
// RAZOR PAGES
// =========================================

builder.Services.AddRazorPages();


// =========================================
// HTTP CONTEXT
// =========================================

builder.Services.AddHttpContextAccessor();


// =========================================
// SESSION
// =========================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(60);

    options.Cookie.HttpOnly =
        true;

    options.Cookie.IsEssential =
        true;

    options.Cookie.SameSite =
        SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        CookieSecurePolicy.SameAsRequest;
});


// =========================================
// API CLIENTS
// =========================================

builder.Services.AddScoped<
    IAuthApiClient,
    AuthApiClient>();

builder.Services.AddScoped<
    IProductsApiClient,
    ProductsApiClient>();

builder.Services.AddScoped<
    ICartApiClient,
    CartApiClient>();

builder.Services.AddScoped<
    IPriceApiClient,
    PriceApiClient>();

builder.Services.AddScoped<
    IOrderApiClient,
    OrderApiClient>();


// =========================================
// JWT FORWARDING HANDLER
// =========================================

builder.Services.AddTransient<
    AuthenticationTokenHandler>();


// =========================================
// PUBLIC GATEWAY CLIENT
// Used for Login/Register etc.
// =========================================

builder.Services.AddHttpClient(
    "GatewayPublic",
    client =>
    {
        client.BaseAddress =
            new Uri(gatewayUrl);

        client.Timeout =
            TimeSpan.FromSeconds(30);
    });


// =========================================
// AUTHORIZED GATEWAY CLIENT
// Automatically forwards JWT
// =========================================

builder.Services
    .AddHttpClient(
        "GatewayAuthorized",
        client =>
        {
            client.BaseAddress =
                new Uri(gatewayUrl);

            client.Timeout =
                TimeSpan.FromSeconds(30);
        })
    .AddHttpMessageHandler<
        AuthenticationTokenHandler>();


// =========================================
// COOKIE AUTHENTICATION
// =========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults
            .AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath =
            "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.LogoutPath =
            "/Account/Logout";

        options.Cookie.Name =
            "BookLoverECommerce.Auth";

        options.Cookie.HttpOnly =
            true;

        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(60);

        options.SlidingExpiration =
            true;
    });


// =========================================
// AUTHORIZATION
// =========================================

builder.Services.AddAuthorization();


// =========================================
// BUILD APP
// =========================================

var app = builder.Build();


// =========================================
// ERROR HANDLING
// =========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error");

    app.UseHsts();
}


// =========================================
// MIDDLEWARE PIPELINE
// =========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// Session must be available before
// pages/API handlers access AccessToken.
app.UseSession();

app.UseAuthentication();

app.UseAuthorization();


// =========================================
// RAZOR PAGES
// =========================================

app.MapRazorPages();

app.Run();