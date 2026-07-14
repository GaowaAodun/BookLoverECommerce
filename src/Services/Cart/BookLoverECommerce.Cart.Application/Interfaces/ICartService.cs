using BookLoverECommerce.Cart.Application.DTOs;

namespace BookLoverECommerce.Cart.Application.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CartResponse> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task<CartResponse?> RemoveItemAsync(
        string userId,
        RemoveCartItemRequest request,
        CancellationToken cancellationToken = default);
}