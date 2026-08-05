using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cart.Application.DTOs;

public class RemoveCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
}