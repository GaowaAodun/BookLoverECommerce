using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Cart;

public sealed class UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}