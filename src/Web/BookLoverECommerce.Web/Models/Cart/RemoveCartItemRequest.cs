using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Cart;

public sealed class RemoveCartItemRequest
{
    public Guid ProductId { get; set; }
}