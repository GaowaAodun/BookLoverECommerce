namespace BookLoverECommerce.Web.Models.Cart;

public sealed class CartViewModel
{
    public int CartId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public List<CartItemViewModel> Items { get; set; } = [];

    public DateTime UpdatedAt { get; set; }

    public int TotalItems =>
        Items.Sum(item => item.Quantity);
}