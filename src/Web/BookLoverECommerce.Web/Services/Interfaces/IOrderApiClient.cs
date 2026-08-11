using BookLoverECommerce.Web.Models.Orders;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface IOrderApiClient
{
    // =========================================
    // CREATE ORDER
    // =========================================

    Task<OrderViewModel> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);


    // =========================================
    // CUSTOMER ORDERS
    // =========================================

    Task<IReadOnlyCollection<OrderViewModel>> GetMyOrdersAsync(
        CancellationToken cancellationToken = default);


    // Compatibility with existing Orders/Index page.
    Task<IReadOnlyCollection<OrderViewModel>> GetOrdersAsync(
        CancellationToken cancellationToken = default);


    // Backend currently does not have GET /orders/{id}.
    // We'll obtain it from /orders/mine.
    Task<OrderViewModel?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);


    // =========================================
    // CUSTOMER ACTIONS
    // =========================================

    Task CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task ConfirmReceiptAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);
}