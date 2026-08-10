namespace BookLoverECommerce.Order.Application.DTOs;

public sealed class ProductSnapshotResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    public int Status { get; init; }
}