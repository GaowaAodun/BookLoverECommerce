using BookLoverECommerce.Products.Application.Products;
using BookLoverECommerce.Products.Application.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Products.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductsApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }

    
}