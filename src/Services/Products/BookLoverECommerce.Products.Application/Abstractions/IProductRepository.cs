using BookLoverECommerce.Products.Domain.Entities;

namespace BookLoverECommerce.Products.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> SkuExistsAsync(
        string sku,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    void Remove(Product product);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}