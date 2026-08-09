namespace BookLoverECommerce.Cms.Models.Products;

public enum ProductType
{
    PrintedBook = 1,
    Clothing = 2,
    Toy = 3,
    Electronics = 4,
    Accessory = 5,
    Gift = 6,
    HomeAndKitchen = 7,
    Other = 8
}

public enum ProductStatus
{
    Draft = 1,
    Published = 2,
    OutOfStock = 3,
    Archived = 4
}

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    string? Brand,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    ProductType ProductType,
    ProductStatus Status,
    string? ThumbnailUrl,
    string CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);