using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Order.Application.DTOs;

public sealed class PaymentSucceededRequest
{
    [Required]
    [StringLength(200)]
    public string PaymentReference { get; init; } = string.Empty;
}