using BookLoverECommerce.Web.Models.Orders;
using BookLoverECommerce.Web.Services.Interfaces;

namespace BookLoverECommerce.Web.Services.ApiClients;

public sealed class OrderApiClient : IOrderApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<OrderViewModel>> GetOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return Array.Empty<OrderViewModel>();
    }

    public async Task<OrderViewModel?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return null;
    }

    public async Task<OrderViewModel> PlaceOrderAsync(
        IReadOnlyList<OrderItemViewModel> items,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        throw new InvalidOperationException(
            "Order API is not connected yet.");
    }
}