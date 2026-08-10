namespace BookLoverECommerce.Order.Application.DTOs;

public sealed class PriceSnapshotResponse
{
    public Guid ProductId { get; init; }

    public decimal BasePrice { get; init; }

    public decimal? SalePrice { get; init; }

    public decimal EffectivePrice { get; init; }

    public string Currency { get; init; } = string.Empty;

    public bool IsSaleActive { get; init; }
}