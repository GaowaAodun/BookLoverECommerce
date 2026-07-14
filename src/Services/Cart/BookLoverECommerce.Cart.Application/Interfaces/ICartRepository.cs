using BookLoverECommerce.Cart.Domain.Entities;

namespace BookLoverECommerce.Cart.Application.Interfaces;

public interface ICartRepository
{
    Task<ShoppingCart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<ShoppingCart> GetOrCreateAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}