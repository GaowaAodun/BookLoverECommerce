using BookLoverECommerce.Products.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Products.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductsApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}