namespace BookLoverECommerce.Contracts.Tracking;

public sealed record ItemAddedToCart(
    string CustomerId,
    Guid ProductId,
    int Quantity,
    DateTimeOffset OccurredAt);