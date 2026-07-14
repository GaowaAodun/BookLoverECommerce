using BookLoverECommerce.Auth.Domain.Constants;
using BookLoverECommerce.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Auth.Infrastructure.Persistence;

public static class AuthSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in new[]
                 {
                     UserRoles.Admin,
                     UserRoles.Customer
                 })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Could not create role '{roleName}': {errors}");
                }
            }
        }

        var adminEmail =
            configuration["AdminUser:Email"]
            ?? throw new InvalidOperationException(
                "AdminUser:Email is missing.");

        var adminUsername =
            configuration["AdminUser:Username"]
            ?? throw new InvalidOperationException(
                "AdminUser:Username is missing.");

        var adminPassword =
            configuration["AdminUser:Password"]
            ?? throw new InvalidOperationException(
                "AdminUser:Password is missing.");

        var adminFullName =
            configuration["AdminUser:FullName"]
            ?? "BookLover Administrator";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                FullName = adminFullName,
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult =
                await userManager.CreateAsync(admin, adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
        {
            var roleResult =
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not assign Admin role: {errors}");
            }
        }
    }
}