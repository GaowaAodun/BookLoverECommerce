namespace BookLoverECommerce.Cart.Application.DTOs;

public class CartResponse
{
    public int CartId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public IList<CartItemResponse> Items { get; set; } =
        new List<CartItemResponse>();

    public DateTime UpdatedAt { get; set; }
}