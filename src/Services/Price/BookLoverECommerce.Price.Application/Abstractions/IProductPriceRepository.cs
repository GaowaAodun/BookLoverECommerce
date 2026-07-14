using BookLoverECommerce.Price.Domain.Entities;

namespace BookLoverECommerce.Price.Application.Abstractions;

public interface IProductPriceRepository
{
    Task<IReadOnlyCollection<ProductPrice>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ProductPrice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProductPrice?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ProductPrice productPrice,
        CancellationToken cancellationToken = default);

    void Remove(ProductPrice productPrice);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
