namespace BookLoverECommerce.Web.Models.Cart;

public sealed class CartItemViewModel
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}