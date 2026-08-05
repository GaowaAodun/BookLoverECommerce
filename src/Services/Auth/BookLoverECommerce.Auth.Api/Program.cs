using System.Security.Claims;
using System.Text;
using BookLoverECommerce.Auth.Application.Configuration;
using BookLoverECommerce.Auth.Infrastructure;
using BookLoverECommerce.Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSection =
    builder.Configuration.GetSection(
        JwtSettings.SectionName);

var jwtSettings =
    jwtSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT Key, Issuer and Audience are required.");
}

builder.Services.AddAuthInfrastructure(
    builder.Configuration);

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

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "BookLoverECommerce Auth API v1");

        options.RoutePrefix = "swagger";
    });
}

var applyMigrations =
    builder.Configuration.GetValue<bool>(
        "Database:ApplyMigrationsOnStartup");

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    var dbContext = services
        .GetRequiredService<AuthDbContext>();

    // First create/update Identity tables.
    await dbContext.Database.MigrateAsync();

    // Only seed after the tables exist.
    await AuthSeeder.SeedAsync(
        services,
        builder.Configuration);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();