using BookLoverECommerce.Order.Application.DTOs;

namespace BookLoverECommerce.Order.Application.Interfaces;

public interface IProductsClient
{
    Task<ProductSnapshotResponse?> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}