using BookLoverECommerce.Order.Application.DTOs.External;

namespace BookLoverECommerce.Order.Application.Interfaces;

public interface IProductPriceClient
{
    Task<ProductPriceSnapshot?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}