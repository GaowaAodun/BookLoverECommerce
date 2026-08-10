using BookLoverECommerce.Order.Application.DTOs;


namespace BookLoverECommerce.Order.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(
        string customerId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrderDto>> GetMineAsync(
        string customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrderDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> MarkPaymentSucceededAsync(
    Guid orderId,
    string paymentReference,
    CancellationToken cancellationToken = default);

    Task<bool> StartProcessingAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<bool> ShipAsync(
        Guid orderId,
        ShipOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ConfirmReceiptAsync(
        Guid orderId,
        string customerId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelByCustomerAsync(
        Guid orderId,
        string customerId,
        CancellationToken cancellationToken = default);

}