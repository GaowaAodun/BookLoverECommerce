namespace BookLoverECommerce.Price.Application.DTOs;

public sealed record ProductPriceDto(
    Guid Id,
    Guid ProductId,
    decimal BasePrice,
    decimal? SalePrice,
    decimal EffectivePrice,
    string Currency,
    bool IsSaleActive,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);
