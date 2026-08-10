using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Order.Application.DTOs;

public sealed class CreateOrderItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
    public int Quantity { get; init; }
}