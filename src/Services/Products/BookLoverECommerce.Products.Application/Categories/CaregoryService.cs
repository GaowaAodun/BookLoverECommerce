using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Application.DTOs;

namespace BookLoverECommerce.Products.Application.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var categories =
            await _categoryRepository.GetActiveAsync(
                cancellationToken);

        return categories
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.DisplayOrder,
                category.ParentCategoryId))
            .ToArray();
    }
}