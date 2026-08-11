namespace BookLoverECommerce.Contracts.Tracking;

public sealed record OrderPlaced(
    Guid OrderId,
    string CustomerId,
    IReadOnlyCollection<OrderPlacedItem> Items,
    decimal TotalAmount,
    DateTimeOffset OccurredAt);

public sealed record OrderPlacedItem(
    Guid ProductId,
    int Quantity);