namespace BookLoverECommerce.Order.Domain.Entities;

public sealed class Shipment
{
    private Shipment() { }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string Carrier { get; private set; } = string.Empty;

    public string TrackingNumber { get; private set; } = string.Empty;

    public string? TrackingUrl { get; private set; }

    public DateTime ShippedAt { get; private set; }

    public DateTime? DeliveredAt { get; private set; }

    public Shipment(
        Guid orderId,
        string carrier,
        string trackingNumber,
        string? trackingUrl,
        DateTime shippedAt)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid order ID is required.",
                nameof(orderId));
        }

        if (string.IsNullOrWhiteSpace(carrier))
        {
            throw new ArgumentException(
                "Carrier is required.",
                nameof(carrier));
        }

        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentException(
                "Tracking number is required.",
                nameof(trackingNumber));
        }

        if (shippedAt == default)
        {
            throw new ArgumentException(
                "A valid shipment time is required.",
                nameof(shippedAt));
        }

        Id = Guid.NewGuid();
        OrderId = orderId;
        Carrier = carrier.Trim();
        TrackingNumber = trackingNumber.Trim();
        TrackingUrl = string.IsNullOrWhiteSpace(trackingUrl)
            ? null
            : trackingUrl.Trim();
        ShippedAt = shippedAt;
    }

    public void MarkDelivered(DateTime deliveredAt)
    {
        if (DeliveredAt.HasValue)
        {
            throw new InvalidOperationException(
                "The shipment has already been delivered.");
        }

        if (deliveredAt < ShippedAt)
        {
            throw new ArgumentException(
                "Delivery time cannot be earlier than shipment time.",
                nameof(deliveredAt));
        }

        DeliveredAt = deliveredAt;
    }
}