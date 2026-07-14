using BookLoverECommerce.Products.Domain.Enums;

namespace BookLoverECommerce.Products.Application.DTOs;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    ProductType ProductType,
    string CreatedByUserId,
    string? Brand,
    string? ThumbnailUrl);