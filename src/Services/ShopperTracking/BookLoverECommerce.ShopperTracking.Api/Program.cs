using System.Security.Claims;
using System.Text;
using BookLoverECommerce.ShopperTracking.Api.Consumers;
using BookLoverECommerce.ShopperTracking.Application.Configuration;
using BookLoverECommerce.ShopperTracking.Infrastructure;
using BookLoverECommerce.ShopperTracking.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// =========================================================
// CONTROLLERS / OPENAPI
// =========================================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();


// =========================================================
// DATABASE
// =========================================================

builder.Services.AddShopperTrackingInfrastructure(
    builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ShopperTrackingDbContext>();


// =========================================================
// JWT AUTHENTICATION
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

                ClockSkew = TimeSpan.Zero,

                NameClaimType =
                    ClaimTypes.NameIdentifier,

                RoleClaimType =
                    ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();


// =========================================================
// RABBITMQ
// =========================================================

var rabbitMqOptions =
    builder.Configuration
        .GetSection(
            RabbitMqOptions.SectionName)
        .Get<RabbitMqOptions>()
    ?? throw new InvalidOperationException(
        "RabbitMQ configuration is missing.");

builder.Services.AddMassTransit(configuration =>
{
    configuration.SetKebabCaseEndpointNameFormatter();

    configuration.AddConsumer<
        ItemAddedToCartConsumer>();

    configuration.AddConsumer<
        OrderPlacedConsumer>();

    configuration.UsingRabbitMq(
        (context, rabbitMq) =>
        {
            rabbitMq.Host(
                rabbitMqOptions.Host,
                rabbitMqOptions.Port,
                rabbitMqOptions.VirtualHost,
                host =>
                {
                    host.Username(
                        rabbitMqOptions.Username);

                    host.Password(
                        rabbitMqOptions.Password);
                });


            rabbitMq.ReceiveEndpoint(
                "shopper-tracking-item-added",
                endpoint =>
                {
                    endpoint.ConfigureConsumer<
                        ItemAddedToCartConsumer>(
                            context);

                    endpoint.UseMessageRetry(
                        retry =>
                        {
                            retry.Interval(
                                3,
                                TimeSpan.FromSeconds(3));
                        });
                });


            rabbitMq.ReceiveEndpoint(
                "shopper-tracking-order-placed",
                endpoint =>
                {
                    endpoint.ConfigureConsumer<
                        OrderPlacedConsumer>(
                            context);

                    endpoint.UseMessageRetry(
                        retry =>
                        {
                            retry.Interval(
                                3,
                                TimeSpan.FromSeconds(3));
                        });
                });
        });
});


// =========================================================
// BUILD
// =========================================================

var app = builder.Build();


// =========================================================
// OPENAPI
// =========================================================

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "BookLoverECommerce ShopperTracking API v1");

        options.RoutePrefix = "swagger";
    });
}


// =========================================================
// MIGRATIONS
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
            .GetRequiredService<
                ShopperTrackingDbContext>();

    await dbContext.Database.MigrateAsync();
}


// =========================================================
// HTTP PIPELINE
// =========================================================

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();