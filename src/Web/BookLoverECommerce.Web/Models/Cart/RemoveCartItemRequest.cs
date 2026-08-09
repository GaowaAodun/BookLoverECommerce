using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Cart;

public sealed class RemoveCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
}