using BookLoverECommerce.Products.Application.DTOs;

namespace BookLoverECommerce.Products.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}