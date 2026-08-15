using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace BookLoverECommerce.Cms.Models.Prices;

// [JsonConverter(typeof(JsonStringEnumConverter))]
public enum Currency
{
    CAD = 1,
    USD = 2,
    CNY = 3
}

public sealed class ProductPriceResponse
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public decimal BasePrice { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Currency Currency { get; set; }

    public decimal? SalePrice { get; set; }

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    public decimal EffectivePrice { get; set; }

    public bool IsSaleActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public sealed class PriceFormModel : IValidatableObject
{
    [Required(ErrorMessage = "Please select a product.")]
    public Guid? ProductId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999",
        ErrorMessage = "Base price must be greater than zero.")]
    [Display(Name = "Base price")]
    public decimal BasePrice { get; set; }

    [Required]
    public Currency Currency { get; set; } = Currency.CAD;

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999",
        ErrorMessage = "Sale price must be greater than zero.")]
    [Display(Name = "Sale price")]
    public decimal? SalePrice { get; set; }

    [Display(Name = "Sale start date")]
    public DateTime? SaleStartDate { get; set; }

    [Display(Name = "Sale end date")]
    public DateTime? SaleEndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SalePrice.HasValue &&
            SalePrice.Value >= BasePrice)
        {
            yield return new ValidationResult(
                "Sale price must be lower than the base price.",
                [nameof(SalePrice)]);
        }

        if (!SalePrice.HasValue &&
            (SaleStartDate.HasValue || SaleEndDate.HasValue))
        {
            yield return new ValidationResult(
                "Enter a sale price before setting sale dates.",
                [nameof(SalePrice)]);
        }

        if (SaleStartDate.HasValue &&
            SaleEndDate.HasValue &&
            SaleEndDate.Value <= SaleStartDate.Value)
        {
            yield return new ValidationResult(
                "Sale end date must be later than the start date.",
                [nameof(SaleEndDate)]);
        }
    }
}

public sealed record CreateProductPriceRequest(
    Guid ProductId,
    decimal BasePrice,
    Currency Currency,
    decimal? SalePrice,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate);

public sealed record UpdateProductPriceRequest(
    decimal BasePrice,
    Currency Currency,
    decimal? SalePrice,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate);