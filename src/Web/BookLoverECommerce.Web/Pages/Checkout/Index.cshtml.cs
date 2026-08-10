using System.Security.Claims;
using BookLoverECommerce.Web.Models.Checkout;
using BookLoverECommerce.Web.Models.Prices;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Checkout;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly ICartApiClient _cartApiClient;
    private readonly IProductsApiClient _productsApiClient;
    private readonly IPriceApiClient _priceApiClient;

    public IndexModel(
        ICartApiClient cartApiClient,
        IProductsApiClient productsApiClient,
        IPriceApiClient priceApiClient)
    {
        _cartApiClient = cartApiClient;
        _productsApiClient = productsApiClient;
        _priceApiClient = priceApiClient;
    }

    public CheckoutViewModel Checkout { get; private set; } =
        new();

    public string? ProductIds { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        string? productIds,
        CancellationToken cancellationToken)
    {
        ProductIds = productIds;

        // =========================================
        // 1. VALIDATE SELECTED IDS
        // =========================================

        if (string.IsNullOrWhiteSpace(productIds))
        {
            ErrorMessage =
                "No products were selected for checkout.";

            return Page();
        }

        var selectedIds =
            productIds
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Select(value =>
                {
                    return Guid.TryParse(
                        value,
                        out var id)
                        ? id
                        : Guid.Empty;
                })
                .Where(id =>
                    id != Guid.Empty)
                .Distinct()
                .ToHashSet();

        if (selectedIds.Count == 0)
        {
            ErrorMessage =
                "The selected product IDs were invalid.";

            return Page();
        }


        // =========================================
        // 2. GET CURRENT USER
        // =========================================

        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }


        try
        {
            // =====================================
            // 3. GET CART
            // =====================================

            var cart =
                await _cartApiClient.GetCartAsync(
                    userId,
                    cancellationToken);

            if (cart is null)
            {
                ErrorMessage =
                    "Your cart could not be loaded.";

                return Page();
            }


            var selectedCartItems =
                cart.Items
                    .Where(item =>
                        selectedIds.Contains(
                            item.ProductId))
                    .ToList();


            if (selectedCartItems.Count == 0)
            {
                ErrorMessage =
                    "The selected products were not found in your cart.";

                return Page();
            }


            // =====================================
            // 4. CALL PRICE API
            // =====================================

            var priceRequest =
                new PriceQuoteRequest
                {
                    Items =
                        selectedCartItems
                            .Select(item =>
                                new PriceQuoteItemRequest
                                {
                                    ProductId =
                                        item.ProductId,

                                    Quantity =
                                        item.Quantity
                                })
                            .ToList()
                };


            var quote =
                await _priceApiClient.GetQuoteAsync(
                    priceRequest,
                    cancellationToken);


            if (quote is null)
            {
                ErrorMessage =
                    "The Price API returned no pricing information.";

                return Page();
            }


            if (quote.Items.Count == 0)
            {
                ErrorMessage =
                    "The Price API returned no prices for the selected products.";

                return Page();
            }


            // =====================================
            // 5. GET PRODUCT INFORMATION
            // =====================================

            var products =
                (await _productsApiClient
                    .GetProductsAsync(
                        cancellationToken))
                    .ToList();


            if (products.Count == 0)
            {
                ErrorMessage =
                    "Product information could not be loaded.";

                return Page();
            }


            // =====================================
            // 6. MERGE:
            //
            // CART     = quantity
            // PRODUCT  = name/image/category
            // PRICE    = real checkout price
            // =====================================

            foreach (var cartItem in selectedCartItems)
            {
                var product =
                    products.FirstOrDefault(
                        product =>
                            product.Id ==
                            cartItem.ProductId);


                var price =
                    quote.Items.FirstOrDefault(
                        price =>
                            price.ProductId ==
                            cartItem.ProductId);


                if (product is null)
                {
                    continue;
                }


                if (price is null)
                {
                    continue;
                }


                Checkout.Items.Add(
                    new CheckoutItemViewModel
                    {
                        ProductId =
                            cartItem.ProductId,

                        ProductName =
                            product.Title,

                        ImageUrl =
                            product.ImageUrl,

                        Category =
                            product.Category,

                        Quantity =
                            cartItem.Quantity,

                        UnitPrice =
                            price.UnitPrice,

                        LineTotal =
                            price.LineTotal,

                        IsSaleActive =
                            price.IsSaleActive
                    });
            }


            // =====================================
            // 7. VERIFY MERGED ITEMS
            // =====================================

            if (Checkout.Items.Count == 0)
            {
                ErrorMessage =
                    "The cart products could not be matched with Product and Price information.";

                return Page();
            }


            // =====================================
            // 8. FINAL TOTALS
            // =====================================

            Checkout.Subtotal =
                quote.Subtotal;

            Checkout.Shipping =
                0m;

            Checkout.Currency =
                string.IsNullOrWhiteSpace(
                    quote.Currency)
                    ? "CAD"
                    : quote.Currency;


            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"A service could not be reached: {ex.Message}";

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Checkout could not be prepared: {ex.Message}";

            return Page();
        }
    }


    public IActionResult OnPostPlaceOrder(
        string? productIds)
    {
        /*
         * We will connect this to the Order API next.
         *
         * DO NOT remove the products from Cart here yet.
         */

        TempData["SuccessMessage"] =
            "Checkout pricing was verified successfully.";

        return RedirectToPage(
            "/Cart/Index");
    }
}