namespace BookLoverECommerce.Cart.Application.DTOs;

public class CartItemResponse
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}