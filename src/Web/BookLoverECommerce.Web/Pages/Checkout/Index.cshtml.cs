using System.Security.Claims;
using BookLoverECommerce.Web.Models.Checkout;
using BookLoverECommerce.Web.Models.Orders;
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
    private readonly IOrderApiClient _orderApiClient;

    public IndexModel(
        ICartApiClient cartApiClient,
        IProductsApiClient productsApiClient,
        IPriceApiClient priceApiClient,
        IOrderApiClient orderApiClient)
    {
        _cartApiClient = cartApiClient;
        _productsApiClient = productsApiClient;
        _priceApiClient = priceApiClient;
        _orderApiClient = orderApiClient;
    }

    public CheckoutViewModel Checkout { get; private set; } =
        new();

    public string? ProductIds { get; private set; }

    public string? ErrorMessage { get; private set; }


    // =========================================
    // SHIPPING ADDRESS FORM
    // =========================================

    [BindProperty]
    public ShippingAddressRequest ShippingAddress { get; set; } =
        new()
        {
            Country = "Canada"
        };


    // =========================================
    // GET CHECKOUT
    // =========================================

    public async Task<IActionResult> OnGetAsync(
        string? productIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
            ShippingAddress.Country))
        {
            ShippingAddress.Country =
                "Canada";
        }

        await LoadCheckoutAsync(
            productIds,
            cancellationToken);

        return Page();
    }


    // =========================================
    // PLACE ORDER
    // =========================================

    public async Task<IActionResult> OnPostPlaceOrderAsync(
        string? productIds,
        CancellationToken cancellationToken)
    {
        ProductIds = productIds;

        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }


        // If shipping address validation failed,
        // reload checkout so validation messages
        // can be displayed.
        if (!ModelState.IsValid)
        {
            await LoadCheckoutAsync(
                productIds,
                cancellationToken);

            return Page();
        }


        var selectedIds =
            ParseProductIds(
                productIds);

        if (selectedIds.Count == 0)
        {
            ErrorMessage =
                "No products were selected for this order.";

            await LoadCheckoutAsync(
                productIds,
                cancellationToken);

            return Page();
        }


        try
        {
            // =====================================
            // GET CURRENT CART
            // =====================================

            var cart =
                await _cartApiClient.GetCartAsync(
                    userId,
                    cancellationToken);

            if (cart is null ||
                cart.Items.Count == 0)
            {
                ErrorMessage =
                    "Your cart is empty.";

                await LoadCheckoutAsync(
                    productIds,
                    cancellationToken);

                return Page();
            }


            // =====================================
            // ONLY SELECTED CART ITEMS
            // =====================================

            var selectedCartItems =
                cart.Items
                    .Where(item =>
                        selectedIds.Contains(
                            item.ProductId))
                    .ToList();


            if (selectedCartItems.Count == 0)
            {
                ErrorMessage =
                    "The selected products could not be found in your cart.";

                await LoadCheckoutAsync(
                    productIds,
                    cancellationToken);

                return Page();
            }


            // =====================================
            // CREATE ORDER REQUEST
            // =====================================

            var request =
                new CreateOrderRequest
                {
                    Items =
                        selectedCartItems
                            .Select(item =>
                                new CreateOrderItemRequest
                                {
                                    ProductId =
                                        item.ProductId,

                                    Quantity =
                                        item.Quantity
                                })
                            .ToArray(),

                    ShippingAddress =
                        ShippingAddress
                };


            // =====================================
            // CALL ORDER API
            //
            // POST /api/orders
            // =====================================

            var order =
                await _orderApiClient.CreateOrderAsync(
                    request,
                    cancellationToken);


            // =====================================
            // SUCCESS
            // =====================================

            TempData["SuccessMessage"] =
                $"Order {order.OrderNumber} was placed successfully.";


            // For now we DO NOT remove cart items.
            // We will add that after Order creation
            // has been tested successfully.


            // =====================================
            // REDIRECT TO ORDER DETAILS
            // =====================================

            return RedirectToPage(
                "/Orders/Details",
                new
                {
                    id = order.Id
                });
        }
        catch (HttpRequestException exception)
        {
            ErrorMessage =
                $"Order could not be created. {exception.Message}";

            await LoadCheckoutAsync(
                productIds,
                cancellationToken);

            return Page();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Something went wrong while placing your order. " +
                $"{exception.Message}";

            await LoadCheckoutAsync(
                productIds,
                cancellationToken);

            return Page();
        }
    }


    // =========================================
    // LOAD CHECKOUT
    // =========================================

    private async Task LoadCheckoutAsync(
        string? productIds,
        CancellationToken cancellationToken)
    {
        ProductIds =
            productIds;

        Checkout =
            new CheckoutViewModel();


        // =====================================
        // VALIDATE PRODUCT IDS
        // =====================================

        if (string.IsNullOrWhiteSpace(productIds))
        {
            ErrorMessage =
                "No products were selected for checkout.";

            return;
        }


        var selectedIds =
            ParseProductIds(
                productIds);


        if (selectedIds.Count == 0)
        {
            ErrorMessage =
                "The selected product IDs were invalid.";

            return;
        }


        // =====================================
        // CURRENT USER
        // =====================================

        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (string.IsNullOrWhiteSpace(userId))
        {
            ErrorMessage =
                "Your user account could not be identified.";

            return;
        }


        try
        {
            // =================================
            // GET CART
            // =================================

            var cart =
                await _cartApiClient.GetCartAsync(
                    userId,
                    cancellationToken);


            if (cart is null)
            {
                ErrorMessage =
                    "Your cart could not be loaded.";

                return;
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

                return;
            }


            // =================================
            // PRICE QUOTE
            // =================================

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


            if (quote is null ||
                quote.Items.Count == 0)
            {
                ErrorMessage =
                    "Pricing information could not be loaded.";

                return;
            }


            // =================================
            // PRODUCT INFORMATION
            // =================================

            var products =
                (await _productsApiClient
                    .GetProductsAsync(
                        cancellationToken))
                    .ToList();


            // =================================
            // MERGE DATA
            // =================================

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


                if (product is null ||
                    price is null)
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


            if (Checkout.Items.Count == 0)
            {
                ErrorMessage =
                    "The selected products could not be prepared for checkout.";

                return;
            }


            // =================================
            // TOTALS
            // =================================

            Checkout.Subtotal =
                quote.Subtotal;

            Checkout.Shipping =
                0m;

            Checkout.Currency =
                string.IsNullOrWhiteSpace(
                    quote.Currency)
                    ? "CAD"
                    : quote.Currency;
        }
        catch (HttpRequestException exception)
        {
            ErrorMessage =
                $"A service could not be reached: " +
                $"{exception.Message}";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Checkout could not be prepared: " +
                $"{exception.Message}";
        }
    }


    // =========================================
    // PARSE PRODUCT IDS
    // =========================================

    private static HashSet<Guid> ParseProductIds(
        string? productIds)
    {
        if (string.IsNullOrWhiteSpace(productIds))
        {
            return new HashSet<Guid>();
        }


        return productIds
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(value =>
                Guid.TryParse(
                    value,
                    out var id)
                    ? id
                    : Guid.Empty)
            .Where(id =>
                id != Guid.Empty)
            .Distinct()
            .ToHashSet();
    }
}