using BookLoverECommerce.Order.Application.Interfaces;
using BookLoverECommerce.Order.Infrastructure.Clients;
using BookLoverECommerce.Order.Infrastructure.Persistence;
using BookLoverECommerce.Order.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("OrderDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'OrderDatabase' was not found.");

        var productsApiUrl =
            configuration["Services:ProductsApi"]
            ?? throw new InvalidOperationException(
                "Products API URL was not configured.");

        var priceApiUrl =
            configuration["Services:PriceApi"]
            ?? throw new InvalidOperationException(
                "Price API URL was not configured.");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddHttpClient<IProductsClient, ProductsClient>(
            client =>
            {
                client.BaseAddress = new Uri(
                    EnsureTrailingSlash(productsApiUrl));
            });

        services.AddHttpClient<IPriceClient, PriceClient>(
            client =>
            {
                client.BaseAddress = new Uri(
                    EnsureTrailingSlash(priceApiUrl));
            });

        return services;
    }

    private static string EnsureTrailingSlash(string url)
    {
        return url.EndsWith(
            "/",
            StringComparison.Ordinal)
                ? url
                : $"{url}/";
    }
}