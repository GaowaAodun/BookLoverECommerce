using BookLoverECommerce.Auth.Domain.Entities;
using BookLoverECommerce.Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookLoverECommerce.Auth.Application.Configuration;
using BookLoverECommerce.Auth.Application.Interfaces;
using BookLoverECommerce.Auth.Infrastructure.Services;

namespace BookLoverECommerce.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("AuthDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'AuthDatabase' was not found.");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>();

            services.Configure<JwtSettings>(
    configuration.GetSection(
        JwtSettings.SectionName));

services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}