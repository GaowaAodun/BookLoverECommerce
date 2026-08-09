using System.Security.Claims;
using BookLoverECommerce.Web.Models.Cart;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Cart;

[Authorize(Roles = "Customer")]
public sealed class IndexModel : PageModel
{
    private readonly ICartApiClient _cartApiClient;
    private readonly IProductsApiClient _productsApiClient;

    public IndexModel(
    ICartApiClient cartApiClient,
    IProductsApiClient productsApiClient)
    {
        _cartApiClient = cartApiClient;
        _productsApiClient = productsApiClient;
    }
    public List<CartDisplayItemViewModel> DisplayItems
    { get; private set; } = [];

    public decimal CartSubtotal =>
    DisplayItems.Sum(item => item.Subtotal);

    public CartViewModel Cart { get; private set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        try
        {
            Cart =
                await _cartApiClient.GetCartAsync(
                    userId,
                    cancellationToken)
                ?? new CartViewModel
                {
                    UserId = userId
                };
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Your cart could not be loaded. {ex.Message}";
        }
        Cart =
    await _cartApiClient.GetCartAsync(
        userId,
        cancellationToken)
    ?? new CartViewModel
    {
        UserId = userId
    };

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(
    int productId,
    int quantity,
    string? returnUrl,
    CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        quantity = Math.Clamp(quantity, 1, 100);

        try
        {
            await _cartApiClient.AddItemAsync(
                userId,
                new AddCartItemRequest
                {
                    ProductId = productId,
                    Quantity = quantity
                },
                cancellationToken);

            SuccessMessage =
                "Book added to your cart.";

            // Return customer to the page they came from
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Cart/Index");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"The book could not be added. {ex.Message}";

            await LoadCartAsync(
                userId,
                cancellationToken);

            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        quantity = Math.Clamp(quantity, 1, 100);

        try
        {
            await _cartApiClient.UpdateItemAsync(
                userId,
                new UpdateCartItemRequest
                {
                    ProductId = productId,
                    Quantity = quantity
                },
                cancellationToken);

            SuccessMessage =
                "Cart updated.";

            return RedirectToPage();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"The cart could not be updated. {ex.Message}";

            await LoadCartAsync(
                userId,
                cancellationToken);

            return Page();
        }
    }

    public async Task<IActionResult> OnPostRemoveAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        try
        {
            await _cartApiClient.RemoveItemAsync(
                userId,
                new RemoveCartItemRequest
                {
                    ProductId = productId
                },
                cancellationToken);

            SuccessMessage =
                "Book removed from your cart.";

            return RedirectToPage();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"The item could not be removed. {ex.Message}";

            await LoadCartAsync(
                userId,
                cancellationToken);

            return Page();
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }

    private async Task LoadCartAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            Cart =
                await _cartApiClient.GetCartAsync(
                    userId,
                    cancellationToken)
                ?? new CartViewModel
                {
                    UserId = userId
                };
        }
        catch
        {
            Cart =
                new CartViewModel
                {
                    UserId = userId
                };
        }
    }
    private async Task LoadDisplayItemsAsync(
    CancellationToken cancellationToken)
    {
        DisplayItems = [];

        if (Cart.Items.Count == 0)
        {
            return;
        }

        var products =
            await _productsApiClient.GetProductsAsync(
                cancellationToken);

        DisplayItems =
            Cart.Items
                .Select(cartItem =>
                {
                    var product =
                        products.FirstOrDefault(
                            p => p.Id == cartItem.ProductId);

                    return new CartDisplayItemViewModel
                    {
                        ProductId =
                            cartItem.ProductId,

                        Quantity =
                            cartItem.Quantity,

                        Title =
                            product?.Title
                            ?? $"Product #{cartItem.ProductId}",

                        Author =
                            product?.Author,

                        Category =
                            product?.Category,

                        ImageUrl =
                            product?.ImageUrl,

                        Price =
                            product?.Price ?? 0
                    };
                })
                .ToList();
    }
}