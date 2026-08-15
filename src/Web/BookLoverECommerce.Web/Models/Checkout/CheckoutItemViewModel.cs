namespace BookLoverECommerce.Web.Models.Checkout;

public sealed class CheckoutItemViewModel
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public string? ImageUrl { get; set; }

    public string? Category { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public bool IsSaleActive { get; set; }
}