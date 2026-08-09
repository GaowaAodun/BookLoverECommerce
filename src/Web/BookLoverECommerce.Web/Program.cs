using BookLoverECommerce.Web.Services.ApiClients;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using BookLoverECommerce.Web.Services.Handlers;

var builder = WebApplication.CreateBuilder(args);

var gatewayUrl =
    builder.Configuration["ApiGateway:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiGateway:BaseUrl is not configured.");

builder.Services.AddRazorPages();

builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<
    IProductsApiClient,
    ProductsApiClient>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<
    AuthenticationTokenHandler>();

builder.Services.AddHttpClient(
    "GatewayPublic",
    client =>
    {
        client.BaseAddress = new Uri(gatewayUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
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

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";

        options.Cookie.Name =
            "BookLoverECommerce.Auth";

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite =
            SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(60);

        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();