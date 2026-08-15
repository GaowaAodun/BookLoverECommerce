using System.Text;
using BookLoverECommerce.Order.Api.OpenApi;
using BookLoverECommerce.Order.Application;
using BookLoverECommerce.Order.Infrastructure;
using BookLoverECommerce.Order.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// =========================================================
// CONTROLLERS / OPENAPI
// =========================================================

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<
        BearerSecuritySchemeTransformer>();
});


// =========================================================
// APPLICATION / INFRASTRUCTURE
// =========================================================

builder.Services.AddOrderApplication();

builder.Services.AddOrderInfrastructure(
    builder.Configuration);


// =========================================================
// HEALTH CHECKS
// =========================================================

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>();


// =========================================================
// JWT
// =========================================================

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer was not configured.");

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience was not configured.");

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key was not configured.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtKey)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();


// =========================================================
// MASSTRANSIT / RABBITMQ
// IMPORTANT: BEFORE builder.Build()
// =========================================================

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var host =
            builder.Configuration["RabbitMq:Host"]
            ?? "rabbitmq";

        var port =
            builder.Configuration.GetValue<ushort?>(
                "RabbitMq:Port")
            ?? 5672;

        var virtualHost =
            builder.Configuration["RabbitMq:VirtualHost"]
            ?? "/";

        var username =
            builder.Configuration["RabbitMq:Username"]
            ?? "booklover";

        var password =
            builder.Configuration["RabbitMq:Password"]
            ?? throw new InvalidOperationException(
                "RabbitMQ password was not configured.");

        cfg.Host(
            host,
            port,
            virtualHost,
            h =>
            {
                h.Username(username);
                h.Password(password);
            });
    });
});


// =========================================================
// BUILD
// =========================================================

var app = builder.Build();


// =========================================================
// SWAGGER
// =========================================================

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "BookLoverECommerce Order API v1");
    });
}


// =========================================================
// DATABASE MIGRATIONS
// =========================================================

var applyMigrations =
    builder.Configuration.GetValue<bool>(
        "Database:ApplyMigrationsOnStartup");

if (applyMigrations)
{
    using var scope =
        app.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

    await dbContext.Database.MigrateAsync();
}


// =========================================================
// HTTP PIPELINE
// =========================================================

// Docker is HTTP-only in your compose setup.
// Avoid redirect problems there.
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();