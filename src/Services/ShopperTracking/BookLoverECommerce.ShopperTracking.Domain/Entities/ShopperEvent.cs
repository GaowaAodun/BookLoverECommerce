namespace BookLoverECommerce.ShopperTracking.Domain.Entities;

public sealed class ShopperEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CustomerId { get; set; } =
        string.Empty;

    public string EventType { get; set; } =
        string.Empty;

    public Guid? ProductId { get; set; }

    public Guid? OrderId { get; set; }

    public int? Quantity { get; set; }

    public DateTime OccurredAt { get; set; } =
        DateTime.UtcNow;

    public string? Metadata { get; set; }
}