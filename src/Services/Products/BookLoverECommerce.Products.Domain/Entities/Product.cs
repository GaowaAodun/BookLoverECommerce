using BookLoverECommerce.Products.Domain.Enums;

namespace BookLoverECommerce.Products.Domain.Entities;

public class Product
{
    private Product()
    {
    }

    public Product(
        string name,
        string description,
        string sku,
        decimal price,
        int stockQuantity,
        int categoryId,
        ProductType productType,
        string createdByUserId,
        string? brand = null,
        string? thumbnailUrl = null)
    {
        SetName(name);
        SetDescription(description);
        SetSku(sku);
        SetPrice(price);
        SetStockQuantity(stockQuantity);
        SetBrand(brand);
        SetThumbnailUrl(thumbnailUrl);
        SetCreatedByUserId(createdByUserId);

        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(categoryId),
                "A valid category ID is required.");
        }

        CategoryId = categoryId;
        ProductType = productType;
        Status = ProductStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public string? Brand { get; private set; }

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public int CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public ProductType ProductType { get; private set; }

    public ProductStatus Status { get; private set; }

    public string? ThumbnailUrl { get; private set; }

    public string CreatedByUserId { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public void UpdateDetails(
        string name,
        string description,
        decimal price,
        int categoryId,
        ProductType productType,
        string? brand,
        string? thumbnailUrl)
    {
        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived product cannot be updated.");
        }

        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(categoryId),
                "A valid category ID is required.");
        }

        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetBrand(brand);
        SetThumbnailUrl(thumbnailUrl);

        CategoryId = categoryId;
        ProductType = productType;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangePrice(decimal price)
    {
        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException(
                "The price of an archived product cannot be changed.");
        }

        SetPrice(price);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateStock(int stockQuantity)
    {
        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException(
                "The stock of an archived product cannot be changed.");
        }

        SetStockQuantity(stockQuantity);

        if (stockQuantity == 0 &&
            Status == ProductStatus.Published)
        {
            Status = ProductStatus.OutOfStock;
        }
        else if (stockQuantity > 0 &&
                 Status == ProductStatus.OutOfStock)
        {
            Status = ProductStatus.Published;
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived product cannot be published.");
        }

        Status = StockQuantity == 0
            ? ProductStatus.OutOfStock
            : ProductStatus.Published;

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MoveToDraft()
    {
        if (Status == ProductStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived product cannot be moved back to draft.");
        }

        Status = ProductStatus.Draft;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == ProductStatus.Archived)
        {
            return;
        }

        Status = ProductStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Product name is required.",
                nameof(name));
        }

        if (name.Length > 200)
        {
            throw new ArgumentException(
                "Product name cannot exceed 200 characters.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Product description is required.",
                nameof(description));
        }

        if (description.Length > 2000)
        {
            throw new ArgumentException(
                "Product description cannot exceed 2000 characters.",
                nameof(description));
        }

        Description = description.Trim();
    }

    private void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException(
                "Product SKU is required.",
                nameof(sku));
        }

        if (sku.Length > 50)
        {
            throw new ArgumentException(
                "Product SKU cannot exceed 50 characters.",
                nameof(sku));
        }

        Sku = sku.Trim().ToUpperInvariant();
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Product price cannot be negative.");
        }

        Price = decimal.Round(
            price,
            2,
            MidpointRounding.AwayFromZero);
    }

    private void SetStockQuantity(int stockQuantity)
    {
        if (stockQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stockQuantity),
                "Stock quantity cannot be negative.");
        }

        StockQuantity = stockQuantity;
    }

    private void SetBrand(string? brand)
    {
        if (brand is not null && brand.Length > 100)
        {
            throw new ArgumentException(
                "Brand cannot exceed 100 characters.",
                nameof(brand));
        }

        Brand = string.IsNullOrWhiteSpace(brand)
            ? null
            : brand.Trim();
    }

    private void SetThumbnailUrl(string? thumbnailUrl)
    {
        if (thumbnailUrl is not null &&
            thumbnailUrl.Length > 500)
        {
            throw new ArgumentException(
                "Thumbnail URL cannot exceed 500 characters.",
                nameof(thumbnailUrl));
        }

        ThumbnailUrl = string.IsNullOrWhiteSpace(thumbnailUrl)
            ? null
            : thumbnailUrl.Trim();
    }

    private void SetCreatedByUserId(string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            throw new ArgumentException(
                "The ID of the administrator creating the product is required.",
                nameof(createdByUserId));
        }

        CreatedByUserId = createdByUserId.Trim();
    }
}