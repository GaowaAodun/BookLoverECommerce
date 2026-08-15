using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Price.Application.DTOs;

public sealed class PriceQuoteItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}