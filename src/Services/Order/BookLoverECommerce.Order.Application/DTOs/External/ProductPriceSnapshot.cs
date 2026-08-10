namespace BookLoverECommerce.Order.Application.DTOs.External;

public sealed record ProductPriceSnapshot(
    Guid ProductId,
    decimal EffectivePrice,
    string Currency);