namespace BookLoverECommerce.Web.Models.Cart;

public sealed class CartItemViewModel
{
    public int ProductId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Author { get; set; }

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal =>
        Price * Quantity;
}