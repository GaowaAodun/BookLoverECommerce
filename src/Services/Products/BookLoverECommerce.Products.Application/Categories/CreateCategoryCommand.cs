namespace BookLoverECommerce.Products.Application.Categories;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder,
    int? ParentCategoryId);