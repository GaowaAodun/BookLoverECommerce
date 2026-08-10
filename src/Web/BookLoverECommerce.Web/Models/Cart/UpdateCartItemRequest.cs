using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Cart;

public sealed class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}