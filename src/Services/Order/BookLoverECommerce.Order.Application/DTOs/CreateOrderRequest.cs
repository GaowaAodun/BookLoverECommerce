using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Order.Application.DTOs;

public sealed class CreateOrderRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
    public IReadOnlyCollection<CreateOrderItemRequest> Items { get; init; }
        = Array.Empty<CreateOrderItemRequest>();

    [Required]
    public required ShippingAddressRequest ShippingAddress { get; init; }
}



