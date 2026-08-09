namespace BookLoverECommerce.Web.Models.Cart;

public sealed class CartDisplayItemViewModel
{
    public int ProductId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Author { get; set; }

    public string? Category { get; set; }

    public string? ImageUrl { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Subtotal =>
        Price * Quantity;
}