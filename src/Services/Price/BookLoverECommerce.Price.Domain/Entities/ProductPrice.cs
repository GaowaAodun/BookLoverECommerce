using BookLoverECommerce.Price.Domain.Enums;

namespace BookLoverECommerce.Price.Domain.Entities;

public sealed class ProductPrice
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal BasePrice { get; private set; }

    public decimal? SalePrice { get; private set; }

    public Currency Currency { get; private set; }

    public DateTime? SaleStartDate { get; private set; }

    public DateTime? SaleEndDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private ProductPrice()
    {
    }

    public ProductPrice(
        Guid productId,
        decimal basePrice,
        Currency currency,
        decimal? salePrice = null,
        DateTime? saleStartDate = null,
        DateTime? saleEndDate = null)
    {
        Validate(
            productId,
            basePrice,
            salePrice,
            saleStartDate,
            saleEndDate);

        Id = Guid.NewGuid();
        ProductId = productId;
        BasePrice = basePrice;
        Currency = currency;
        SalePrice = salePrice;
        SaleStartDate = saleStartDate;
        SaleEndDate = saleEndDate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetEffectivePrice(DateTime utcNow)
    {
        return IsSaleActive(utcNow)
            ? SalePrice!.Value
            : BasePrice;
    }

    public bool IsSaleActive(DateTime utcNow)
    {
        if (!SalePrice.HasValue)
        {
            return false;
        }

        var hasStarted =
            !SaleStartDate.HasValue ||
            utcNow >= SaleStartDate.Value;

        var hasNotEnded =
            !SaleEndDate.HasValue ||
            utcNow <= SaleEndDate.Value;

        return hasStarted && hasNotEnded;
    }

    public void Update(
        decimal basePrice,
        Currency currency,
        decimal? salePrice,
        DateTime? saleStartDate,
        DateTime? saleEndDate)
    {
        Validate(
            ProductId,
            basePrice,
            salePrice,
            saleStartDate,
            saleEndDate);

        BasePrice = basePrice;
        Currency = currency;
        SalePrice = salePrice;
        SaleStartDate = saleStartDate;
        SaleEndDate = saleEndDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(
        Guid productId,
        decimal basePrice,
        decimal? salePrice,
        DateTime? saleStartDate,
        DateTime? saleEndDate)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(productId));
        }

        if (basePrice <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(basePrice),
                "Base price must be greater than zero.");
        }

        if (salePrice.HasValue)
        {
            if (salePrice.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(salePrice),
                    "Sale price must be greater than zero.");
            }

            if (salePrice.Value >= basePrice)
            {
                throw new ArgumentException(
                    "Sale price must be lower than the base price.",
                    nameof(salePrice));
            }
        }

        if (!salePrice.HasValue &&
            (saleStartDate.HasValue || saleEndDate.HasValue))
        {
            throw new ArgumentException(
                "Sale dates cannot be provided without a sale price.");
        }

        if (saleStartDate.HasValue &&
            saleEndDate.HasValue &&
            saleEndDate.Value <= saleStartDate.Value)
        {
            throw new ArgumentException(
                "Sale end date must be later than the sale start date.",
                nameof(saleEndDate));
        }
    }
}
