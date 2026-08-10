using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cart.Application.DTOs;

public class AddCartItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}