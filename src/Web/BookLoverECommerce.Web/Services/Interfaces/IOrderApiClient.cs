using BookLoverECommerce.Web.Models.Orders;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface IOrderApiClient
{
    Task<IReadOnlyList<OrderViewModel>> GetOrdersAsync(
        CancellationToken cancellationToken = default);

    Task<OrderViewModel?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<OrderViewModel> PlaceOrderAsync(
        IReadOnlyList<OrderItemViewModel> items,
        CancellationToken cancellationToken = default);
}