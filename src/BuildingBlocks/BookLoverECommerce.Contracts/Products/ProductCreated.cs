namespace BookLoverECommerce.Contracts.Products;

public sealed record ProductCreated(
    int ProductId,
    string Name,
    string Sku,
    decimal Price,
    DateTimeOffset CreatedAtUtc);