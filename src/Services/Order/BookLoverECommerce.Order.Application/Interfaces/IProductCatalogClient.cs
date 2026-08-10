using BookLoverECommerce.Order.Application.DTOs.External;

namespace BookLoverECommerce.Order.Application.Interfaces;

public interface IProductCatalogClient
{
    Task<ProductSnapshot?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}