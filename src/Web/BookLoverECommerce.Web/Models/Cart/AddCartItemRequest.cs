namespace BookLoverECommerce.Web.Models.Cart;

public sealed class AddCartItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; } = 1;
}