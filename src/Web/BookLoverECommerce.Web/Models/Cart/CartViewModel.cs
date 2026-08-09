namespace BookLoverECommerce.Web.Models.Cart;

public sealed class CartViewModel
{
    public string UserId { get; set; } = string.Empty;

    public List<CartItemViewModel> Items { get; set; } = [];

    public int TotalItems =>
        Items.Sum(item => item.Quantity);

    public decimal Total =>
        Items.Sum(item => item.Subtotal);
}