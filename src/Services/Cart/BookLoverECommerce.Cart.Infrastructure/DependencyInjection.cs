using BookLoverECommerce.Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookLoverECommerce.Cart.Application.Interfaces;
using BookLoverECommerce.Cart.Infrastructure.Repositories;
using BookLoverECommerce.Cart.Infrastructure.Services;

namespace BookLoverECommerce.Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCartInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("CartDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'CartDatabase' was not found.");

        services.AddDbContext<CartDbContext>(options =>
            options.UseNpgsql(connectionString));

            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartService>();

        return services;
    }
}