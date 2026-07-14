namespace BookLoverECommerce.Cart.Domain.Entities;

public class CartItem
{
    public int Id { get; set; }

    public int ShoppingCartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public ShoppingCart ShoppingCart { get; set; } = null!;
}