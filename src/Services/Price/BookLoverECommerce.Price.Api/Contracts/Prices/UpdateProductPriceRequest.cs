using BookLoverECommerce.Price.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Price.Api.Contracts.Prices;

public sealed class UpdateProductPriceRequest
{
    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal BasePrice { get; init; }

    public Currency Currency { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal? SalePrice { get; init; }

    public DateTime? SaleStartDate { get; init; }

    public DateTime? SaleEndDate { get; init; }
}
