using BookLoverECommerce.Products.Application.DTOs;

namespace BookLoverECommerce.Products.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default);

    Task<CategoryDto> GetByIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<CategoryDto> CreateAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default);

    Task<CategoryDto> UpdateAsync(
        int categoryId,
        UpdateCategoryCommand command,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int categoryId,
        CancellationToken cancellationToken = default);
}