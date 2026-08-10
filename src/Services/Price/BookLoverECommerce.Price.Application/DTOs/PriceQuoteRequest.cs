using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Price.Application.DTOs;

public sealed class PriceQuoteRequest
{
    [Required]
    [MinLength(1)]
    public List<PriceQuoteItemRequest> Items { get; set; } =
        new();
}