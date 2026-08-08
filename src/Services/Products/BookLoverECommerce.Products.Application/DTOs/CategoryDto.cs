namespace BookLoverECommerce.Products.Application.DTOs;

public sealed record CategoryDto(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    int? ParentCategoryId);