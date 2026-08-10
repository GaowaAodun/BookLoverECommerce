namespace BookLoverECommerce.Web.Models.Orders;

public sealed class OrderViewModel
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string Status { get; set; } =
        "Pending";

    public List<OrderItemViewModel> Items { get; set; } =
        new();

    public decimal Subtotal =>
        Items.Sum(item => item.LineTotal);

    public decimal ShippingAmount { get; set; }

    public decimal Total =>
        Subtotal + ShippingAmount;

    public int TotalQuantity =>
        Items.Sum(item => item.Quantity);
}