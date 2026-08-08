using BookLoverECommerce.Products.Domain.Enums;

namespace BookLoverECommerce.Products.Application.DTOs;

public sealed record UpdateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    ProductType ProductType,
    string? Brand,
    string? ThumbnailUrl);