using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cart.Application.DTOs;

public class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}