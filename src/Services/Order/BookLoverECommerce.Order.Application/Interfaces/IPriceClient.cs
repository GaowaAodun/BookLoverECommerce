using BookLoverECommerce.Order.Application.DTOs;

namespace BookLoverECommerce.Order.Application.Interfaces;

public interface IPriceClient
{
    Task<PriceSnapshotResponse?> GetPriceAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}