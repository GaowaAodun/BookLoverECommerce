using BookLoverECommerce.ShopperTracking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.ShopperTracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddShopperTrackingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "ShopperTrackingDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'ShopperTrackingDatabase' was not found.");

        services.AddDbContext<ShopperTrackingDbContext>(
            options =>
            {
                options.UseNpgsql(connectionString);
            });

        return services;
    }
}