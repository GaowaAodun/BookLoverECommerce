using BookLoverECommerce.Cart.Application.Interfaces;
using BookLoverECommerce.Cart.Domain.Entities;
using BookLoverECommerce.Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Cart.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDbContext _context;

    public CartRepository(CartDbContext context)
    {
        _context = context;
    }

    public async Task<ShoppingCart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ShoppingCarts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.UserId == userId,
                cancellationToken);
    }

    public async Task<ShoppingCart> GetOrCreateAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetByUserIdAsync(
            userId,
            cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new ShoppingCart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ShoppingCarts.AddAsync(
            cart,
            cancellationToken);

        return cart;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}