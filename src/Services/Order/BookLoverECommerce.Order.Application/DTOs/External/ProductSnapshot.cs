namespace BookLoverECommerce.Order.Application.DTOs.External;

public sealed record ProductSnapshot(
    Guid Id,
    string Name,
    int StockQuantity);