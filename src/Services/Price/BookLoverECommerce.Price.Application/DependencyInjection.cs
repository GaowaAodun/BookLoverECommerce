using BookLoverECommerce.Price.Application.Prices;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Price.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPriceApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IProductPriceService, ProductPriceService>();

        return services;
    }
}
