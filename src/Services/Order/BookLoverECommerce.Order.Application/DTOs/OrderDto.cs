namespace BookLoverECommerce.Order.Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<OrderItemDto> Items);