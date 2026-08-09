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

    public CartViewModel Cart { get; private set; } =
        new();

    public List<CartDisplayItemViewModel> DisplayItems
        { get; private set; } = [];

    public decimal CartSubtotal =>
        DisplayItems.Sum(item => item.Subtotal);

    [TempData]
    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; private set; }


    // =====================================
    // LOAD CART PAGE
    // =====================================

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
            // 1. Get cart
            Cart =
                await _cartApiClient.GetCartAsync(
                    userId,
                    cancellationToken)
                ?? new CartViewModel
                {
                    UserId = userId
                };

            // 2. Get Product information
            await LoadDisplayItemsAsync(
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Your cart could not be loaded. {ex.Message}";
        }

        return Page();
    }


    // =====================================
    // ADD ITEM
    // =====================================

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

        quantity =
            Math.Clamp(quantity, 1, 100);

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
                "Product added to your cart.";

            if (!string.IsNullOrWhiteSpace(returnUrl)
                &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Cart/Index");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"The product could not be added. {ex.Message}";

            await ReloadEverythingAsync(
                userId,
                cancellationToken);

            return Page();
        }
    }


    // =====================================
    // UPDATE ITEM
    // =====================================

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

        quantity =
            Math.Clamp(quantity, 1, 100);

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

            await ReloadEverythingAsync(
                userId,
                cancellationToken);

            return Page();
        }
    }


    // =====================================
    // REMOVE ITEM
    // =====================================

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
                "Product removed from your cart.";

            return RedirectToPage();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"The product could not be removed. {ex.Message}";

            await ReloadEverythingAsync(
                userId,
                cancellationToken);

            return Page();
        }
    }


    // =====================================
    // CURRENT USER
    // =====================================

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }


    // =====================================
    // RELOAD CART + PRODUCTS
    // =====================================

    private async Task ReloadEverythingAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        Cart =
            await _cartApiClient.GetCartAsync(
                userId,
                cancellationToken)
            ?? new CartViewModel
            {
                UserId = userId
            };

        await LoadDisplayItemsAsync(
            cancellationToken);
    }


    // =====================================
    // ENRICH CART ITEMS
    // =====================================

    private async Task LoadDisplayItemsAsync(
        CancellationToken cancellationToken)
    {
        DisplayItems = [];

        if (Cart.Items is null ||
            Cart.Items.Count == 0)
        {
            return;
        }

        var products =
            await _productsApiClient.GetProductsAsync(
                cancellationToken);

        foreach (var cartItem in Cart.Items)
        {
            var product =
                products.FirstOrDefault(
                    p => p.Id == cartItem.ProductId);

            DisplayItems.Add(
                new CartDisplayItemViewModel
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
                });
        }
    }
}