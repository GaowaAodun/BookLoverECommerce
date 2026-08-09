namespace BookLoverECommerce.Products.Application.Categories;

public sealed record UpdateCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder,
    int? ParentCategoryId);