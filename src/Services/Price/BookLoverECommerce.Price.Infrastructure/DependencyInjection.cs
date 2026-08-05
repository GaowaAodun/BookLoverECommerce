using BookLoverECommerce.Price.Application.Abstractions;
using BookLoverECommerce.Price.Infrastructure.Persistence;
using BookLoverECommerce.Price.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Price.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPriceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "PriceDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'PriceDatabase' was not found.");
        }

        services.AddDbContext<PriceDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<
            IProductPriceRepository,
            ProductPriceRepository>();

        return services;
    }
}
