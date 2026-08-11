namespace BookLoverECommerce.Web.Models.Orders;

public sealed class OrderViewModel
{
    // =========================================
    // VALUES RETURNED BY ORDER API
    // =========================================

    public Guid Id { get; set; }

    public string CustomerId { get; set; } =
        string.Empty;

    public string Status { get; set; } =
        string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyCollection<OrderItemViewModel> Items
        { get; set; } =
        Array.Empty<OrderItemViewModel>();


    // =========================================
    // FRONTEND COMPATIBILITY PROPERTIES
    // =========================================

    // Existing UI expects OrderNumber.
    // Backend currently only provides Guid Id.
    public string OrderNumber =>
        $"ORD-{Id.ToString()[..8].ToUpperInvariant()}";


    // Existing UI expects CreatedAtUtc.
    public DateTime CreatedAtUtc =>
        CreatedAt;


    // Total number of books/items in the order.
    public int TotalQuantity =>
        Items.Sum(item => item.Quantity);


    // Current backend doesn't provide shipping separately.
    // For now shipping is treated as free.
    public decimal ShippingAmount =>
        0m;


    // Sum of all order lines.
    public decimal Subtotal =>
        Items.Sum(item => item.Subtotal);


    // Existing Razor page calls this Total.
    public decimal Total =>
        TotalAmount;
}