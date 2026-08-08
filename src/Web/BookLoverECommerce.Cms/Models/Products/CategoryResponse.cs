namespace BookLoverECommerce.Cms.Models.Products;

public sealed record CategoryResponse(
    int Id,
    string Name,
    string? Description,
    int DisplayOrder,
    int? ParentCategoryId);