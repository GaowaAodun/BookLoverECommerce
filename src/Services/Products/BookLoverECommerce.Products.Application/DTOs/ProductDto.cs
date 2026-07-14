using BookLoverECommerce.Products.Domain.Enums;

namespace BookLoverECommerce.Products.Application.DTOs;

public sealed record ProductDto(
    int Id,
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