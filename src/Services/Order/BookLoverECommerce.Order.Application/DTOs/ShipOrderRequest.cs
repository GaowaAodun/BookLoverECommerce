using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Order.Application.DTOs;

public sealed class ShipOrderRequest
{
    [Required]
    [StringLength(100)]
    public string Carrier { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TrackingNumber { get; init; } = string.Empty;

    [StringLength(500)]
    [Url]
    public string? TrackingUrl { get; init; }
}