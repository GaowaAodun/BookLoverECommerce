using BookLoverECommerce.Order.Application.Interfaces;
using BookLoverECommerce.Order.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookLoverECommerce.Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}