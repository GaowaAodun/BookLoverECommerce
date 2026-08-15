using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Prices;
using BookLoverECommerce.Cms.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookLoverECommerce.Cms.Pages.Prices;

[Authorize(Policy = "AdminOnly")]
public sealed class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IHttpClientFactory httpClientFactory,
        ILogger<CreateModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty]
    public PriceFormModel Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> ProductOptions
    {
        get;
        private set;
    } = Array.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(
        Guid? productId,
        CancellationToken cancellationToken)
    {
        Input.ProductId = productId;

        var result =
            await LoadProductsAsync(cancellationToken);

        return result ?? Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var loadResult =
                await LoadProductsAsync(cancellationToken);

            return loadResult ?? Page();
        }

        var request = new CreateProductPriceRequest(
            Input.ProductId!.Value,
            Input.BasePrice,
            Input.Currency,
            Input.SalePrice,
            Input.SaleStartDate,
            Input.SaleEndDate);

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/prices",
                request,
                cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account does not have permission to create prices.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await ReadErrorMessageAsync(
                        response,
                        "The price could not be created.",
                        cancellationToken));
            }
            else
            {
                TempData["SuccessMessage"] =
                    "The price was created successfully.";

                return RedirectToPage("/Prices/Index");
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Creating a product price failed.");

            ModelState.AddModelError(
                string.Empty,
                "The Price service is currently unavailable.");
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Creating a product price timed out.");

            ModelState.AddModelError(
                string.Empty,
                "The Price service took too long to respond.");
        }

        var result =
            await LoadProductsAsync(cancellationToken);

        return result ?? Page();
    }

    private async Task<IActionResult?> LoadProductsAsync(
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.GetAsync(
                "api/products/admin",
                cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Products could not be loaded.");

                return null;
            }

            var products =
                await response.Content.ReadFromJsonAsync<
                    List<ProductResponse>>(
                    cancellationToken: cancellationToken)
                ?? [];

            ProductOptions = products
                .OrderBy(product => product.Name)
                .Select(product => new SelectListItem(
                    $"{product.Name} ({product.Sku})",
                    product.Id.ToString()))
                .ToList();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Loading products for the price form failed.");

            ModelState.AddModelError(
                string.Empty,
                "The Products service is currently unavailable.");
        }

        return null;
    }

    private IActionResult RedirectToLogin()
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page("/Prices/Create")
            });
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var error =
                await response.Content.ReadFromJsonAsync<
                    ApiErrorResponse>(
                    cancellationToken: cancellationToken);

            return string.IsNullOrWhiteSpace(error?.Message)
                ? defaultMessage
                : error.Message;
        }
        catch
        {
            return defaultMessage;
        }
    }

    private sealed record ApiErrorResponse(string? Message);
}