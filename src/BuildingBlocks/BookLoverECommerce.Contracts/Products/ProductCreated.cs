namespace BookLoverECommerce.Contracts.Products;

public sealed record ProductCreated(
    Guid ProductId,
    string Name,
    string Sku,
    decimal Price,
    DateTimeOffset CreatedAtUtc);