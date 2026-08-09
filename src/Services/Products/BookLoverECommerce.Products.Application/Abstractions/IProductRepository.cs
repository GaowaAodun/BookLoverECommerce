using BookLoverECommerce.Products.Domain.Entities;

namespace BookLoverECommerce.Products.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        Guid id,
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