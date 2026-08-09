namespace BookLoverECommerce.Cms.Models.Categories;

public sealed record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    int? ParentCategoryId);