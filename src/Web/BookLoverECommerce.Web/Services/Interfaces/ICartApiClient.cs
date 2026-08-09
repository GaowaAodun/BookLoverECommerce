using BookLoverECommerce.Web.Models.Cart;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface ICartApiClient
{
    Task<CartViewModel?> GetCartAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CartViewModel?> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task<CartViewModel?> UpdateItemAsync(
        string userId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task<CartViewModel?> RemoveItemAsync(
        string userId,
        RemoveCartItemRequest request,
        CancellationToken cancellationToken = default);
}