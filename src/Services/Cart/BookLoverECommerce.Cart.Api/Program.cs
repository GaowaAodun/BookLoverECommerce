using System.Security.Claims;
using System.Text;
using BookLoverECommerce.Cart.Api.OpenApi;
using BookLoverECommerce.Cart.Application.Configuration;
using BookLoverECommerce.Cart.Infrastructure;
using BookLoverECommerce.Cart.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// =========================================================
// JWT CONFIGURATION
// =========================================================

var jwtSettings =
    builder.Configuration
        .GetSection(JwtSettings.SectionName)
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT Key, Issuer and Audience are required.");
}


// =========================================================
// INFRASTRUCTURE
// =========================================================

builder.Services.AddCartInfrastructure(
    builder.Configuration);


// =========================================================
// MASSTRANSIT / RABBITMQ
// =========================================================

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host =
            builder.Configuration["RabbitMq:Host"]
            ?? "rabbitmq";

        var username =
            builder.Configuration["RabbitMq:Username"]
            ?? "booklover";

        var password =
            builder.Configuration["RabbitMq:Password"]
            ?? throw new InvalidOperationException(
                "RabbitMQ password is missing.");

        cfg.Host(
            host,
            "/",
            h =>
            {
                h.Username(username);
                h.Password(password);
            });
    });
});


// =========================================================
// AUTHENTICATION
// =========================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key)),

                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType =
                    ClaimTypes.NameIdentifier,

                RoleClaimType =
                    ClaimTypes.Role
            };

        // Temporary debugging
        options.Events =
            new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    Console.WriteLine(
                        $"CART TOKEN: {context.Token}");

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine(
                        $"CART JWT AUTH FAILED: " +
                        $"{context.Exception.GetType().Name} - " +
                        $"{context.Exception.Message}");

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    Console.WriteLine(
                        $"CART JWT VALIDATED: " +
                        $"{context.Principal?.Identity?.Name}");

                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    Console.WriteLine(
                        $"CART JWT CHALLENGE: " +
                        $"Error={context.Error}; " +
                        $"Description={context.ErrorDescription}");

                    return Task.CompletedTask;
                }
            };
    });


// =========================================================
// SERVICES
// =========================================================

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<
        BearerSecuritySchemeTransformer>();
});

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<CartDbContext>();


// =========================================================
// BUILD
// =========================================================

var app = builder.Build();


// =========================================================
// OPENAPI / SWAGGER
// =========================================================

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "BookLoverECommerce Cart API v1");

        options.RoutePrefix =
            "swagger";
    });
}


// =========================================================
// HTTP PIPELINE
// =========================================================

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/health");

app.Run();