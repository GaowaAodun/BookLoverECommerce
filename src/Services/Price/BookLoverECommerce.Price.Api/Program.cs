using BookLoverECommerce.Price.Application;
using BookLoverECommerce.Price.Infrastructure;
using BookLoverECommerce.Price.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using BookLoverECommerce.Price.Api.OpenApi;

using BookLoverECommerce.Price.Api.Consumers;
using BookLoverECommerce.Shared.Messaging;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<
        BearerSecuritySchemeTransformer>();
});

builder.Services.AddPriceApplication();

builder.Services.AddPriceInfrastructure(
    builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PriceDbContext>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),

                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

// RabbitMQ Configuration
var rabbitMqOptions = builder.Configuration
    .GetSection(RabbitMqOptions.SectionName)
    .Get<RabbitMqOptions>()
    ?? throw new InvalidOperationException(
        "RabbitMQ configuration is missing.");

builder.Services.AddMassTransit(configuration =>
{
    configuration.SetKebabCaseEndpointNameFormatter();

    configuration.AddConsumer<ProductCreatedConsumer>();

    configuration.UsingRabbitMq((context, rabbitMq) =>
    {
        rabbitMq.Host(
            rabbitMqOptions.Host,
            rabbitMqOptions.Port,
            rabbitMqOptions.VirtualHost,
            host =>
            {
                host.Username(rabbitMqOptions.Username);
                host.Password(rabbitMqOptions.Password);
            });

        rabbitMq.ReceiveEndpoint(
            "price-product-created",
            endpoint =>
            {
                endpoint.ConfigureConsumer<ProductCreatedConsumer>(
                    context);

                endpoint.UseMessageRetry(retry =>
                {
                    retry.Interval(
                        3,
                        TimeSpan.FromSeconds(3));
                });
            });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() ||
    app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "BookLoverECommerce Price API v1");

        options.RoutePrefix = "swagger";
    });
}

var applyMigrations =
    builder.Configuration.GetValue<bool>(
        "Database:ApplyMigrationsOnStartup");

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<PriceDbContext>();

    await dbContext.Database.MigrateAsync();

    // await PriceDataSeeder.SeedAsync(dbContext);
}

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
