using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cart.Application.DTOs;

public class RemoveCartItemRequest
{
    
    public Guid ProductId { get; set; }
}