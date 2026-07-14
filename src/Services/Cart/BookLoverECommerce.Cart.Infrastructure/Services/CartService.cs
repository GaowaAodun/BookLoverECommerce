using BookLoverECommerce.Cart.Application.DTOs;
using BookLoverECommerce.Cart.Application.Interfaces;
using BookLoverECommerce.Cart.Domain.Entities;

namespace BookLoverECommerce.Cart.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _repository;

    public CartService(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<CartResponse> GetCartAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await _repository.GetOrCreateAsync(
            userId,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return Map(cart);
    }

    public async Task<CartResponse> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var cart = await _repository.GetOrCreateAsync(
            userId,
            cancellationToken);

        var existingItem = cart.Items.SingleOrDefault(
            item => item.ProductId == request.ProductId);

        if (existingItem is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }
        else
        {
            existingItem.Quantity += request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return Map(cart);
    }

    public async Task<CartResponse?> RemoveItemAsync(
        string userId,
        RemoveCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var cart = await _repository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            return null;
        }

        var item = cart.Items.SingleOrDefault(
            cartItem => cartItem.ProductId == request.ProductId);

        if (item is null)
        {
            return null;
        }

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return Map(cart);
    }

    private static CartResponse Map(ShoppingCart cart)
    {
        return new CartResponse
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items
                .Select(item => new CartItemResponse
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }
}