namespace BookLoverECommerce.Web.Models.Checkout;

public sealed class CheckoutViewModel
{
    public List<CheckoutItemViewModel> Items { get; set; } =
        new();

    public decimal Subtotal { get; set; }

    public decimal Shipping { get; set; }

    public decimal Total =>
        Subtotal + Shipping;

    public string Currency { get; set; } =
        "CAD";

    public int TotalQuantity =>
        Items.Sum(item => item.Quantity);
}