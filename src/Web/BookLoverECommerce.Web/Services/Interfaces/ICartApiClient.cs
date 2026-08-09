using BookLoverECommerce.Web.Models.Cart;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface ICartApiClient
{
    Task<CartViewModel?> GetCartAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveItemAsync(
        string userId,
        int productId,
        CancellationToken cancellationToken = default);
}