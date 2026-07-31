using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Infrastructure.Persistence;
using BookLoverECommerce.Products.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "ProductsDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ProductsDatabase' was not found.");
        }

        services.AddDbContext<ProductsDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }
}