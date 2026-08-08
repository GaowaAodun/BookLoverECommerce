using BookLoverECommerce.Products.Domain.Entities;

namespace BookLoverECommerce.Products.Application.Abstractions;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}