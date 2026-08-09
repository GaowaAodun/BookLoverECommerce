using BookLoverECommerce.Products.Domain.Entities;

namespace BookLoverECommerce.Products.Application.Abstractions;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        int? excludingCategoryId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasProductsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> HasChildCategoriesAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default);

    void Remove(Category category);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}