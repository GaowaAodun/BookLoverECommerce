using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Prices;
using BookLoverECommerce.Cms.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages.Prices;

[Authorize(Policy = "AdminOnly")]
public sealed class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IHttpClientFactory httpClientFactory,
        ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public IReadOnlyList<PriceListItem> Prices { get; private set; } =
        Array.Empty<PriceListItem>();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var pricesResponse = await client.GetAsync(
                "api/prices",
                cancellationToken);

            if (pricesResponse.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (pricesResponse.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account does not have permission to view prices.";

                return Page();
            }

            if (!pricesResponse.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    pricesResponse,
                    "Prices could not be loaded.",
                    cancellationToken);

                return Page();
            }

            var prices =
                await pricesResponse.Content.ReadFromJsonAsync<
                    List<ProductPriceResponse>>(
                    cancellationToken: cancellationToken)
                ?? [];

            var productNames =
                await LoadProductNamesAsync(
                    client,
                    cancellationToken);

            Prices = prices
                .Select(price => new PriceListItem(
                    price,
                    productNames.TryGetValue(
                        price.ProductId,
                        out var name)
                            ? name
                            : $"Product {price.ProductId}"))
                .OrderBy(item => item.ProductName)
                .ToList();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The Price API request failed.");

            ErrorMessage =
                "The Price service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The Price API request timed out.");

            ErrorMessage =
                "The Price service took too long to respond.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            ErrorMessage = "A valid price ID is required.";
            return RedirectToPage();
        }

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.DeleteAsync(
                $"api/prices/{id}",
                cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode ==
                HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account does not have permission to delete prices.";

                return RedirectToPage();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    "The price could not be deleted.",
                    cancellationToken);

                return RedirectToPage();
            }

            SuccessMessage =
                "The price was deleted successfully.";
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Deleting price {PriceId} failed.",
                id);

            ErrorMessage =
                "The Price service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Deleting price {PriceId} timed out.",
                id);

            ErrorMessage =
                "The Price service took too long to respond.";
        }

        return RedirectToPage();
    }

    private static async Task<Dictionary<Guid, string>>
        LoadProductNamesAsync(
            HttpClient client,
            CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(
            "api/products/admin",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var products =
            await response.Content.ReadFromJsonAsync<
                List<ProductResponse>>(
                cancellationToken: cancellationToken)
            ?? [];

        return products.ToDictionary(
            product => product.Id,
            product => product.Name);
    }

    private IActionResult RedirectToLogin()
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page("/Prices/Index")
            });
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiError =
                await response.Content.ReadFromJsonAsync<
                    ApiErrorResponse>(
                    cancellationToken: cancellationToken);

            return string.IsNullOrWhiteSpace(apiError?.Message)
                ? defaultMessage
                : apiError.Message;
        }
        catch
        {
            return defaultMessage;
        }
    }

    public sealed record PriceListItem(
        ProductPriceResponse Price,
        string ProductName);

    private sealed record ApiErrorResponse(string? Message);
}