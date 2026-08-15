using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Prices;
using BookLoverECommerce.Cms.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages.Prices;

[Authorize(Policy = "AdminOnly")]
public sealed class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IHttpClientFactory httpClientFactory,
        ILogger<EditModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public PriceFormModel Input { get; set; } = new();

    public string ProductName { get; private set; } =
        string.Empty;

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        if (Id == Guid.Empty)
        {
            return NotFound();
        }

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.GetAsync(
                $"api/prices/{Id}",
                cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The price could not be loaded.");

                return Page();
            }

            var price =
                await response.Content.ReadFromJsonAsync<
                    ProductPriceResponse>(
                    cancellationToken: cancellationToken);

            if (price is null)
            {
                return NotFound();
            }

            Input = new PriceFormModel
            {
                ProductId = price.ProductId,
                BasePrice = price.BasePrice,
                Currency = price.Currency,
                SalePrice = price.SalePrice,
                SaleStartDate =
                    price.SaleStartDate?.ToLocalTime(),
                SaleEndDate =
                    price.SaleEndDate?.ToLocalTime()
            };

            await LoadProductNameAsync(
                client,
                price.ProductId,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Loading price {PriceId} failed.",
                Id);

            ModelState.AddModelError(
                string.Empty,
                "The Price service is currently unavailable.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        ModelState.Remove("Input.ProductId");

        if (Id == Guid.Empty)
        {
            return NotFound();
        }

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        if (!ModelState.IsValid)
        {
            if (Input.ProductId.HasValue)
            {
                await LoadProductNameAsync(
                    client,
                    Input.ProductId.Value,
                    cancellationToken);
            }

            return Page();
        }

        var request = new UpdateProductPriceRequest(
            Input.BasePrice,
            Input.Currency,
            Input.SalePrice,
            Input.SaleStartDate,
            Input.SaleEndDate);

        try
        {
            var token = HttpContext.Session.GetString("AccessToken");

            _logger.LogInformation(
                "Updating price {PriceId}. Access token exists: {TokenExists}",
                Id,
                !string.IsNullOrWhiteSpace(token));
            using var response = await client.PutAsJsonAsync(
                $"api/prices/{Id}",
                request,
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
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
                    "Your account does not have permission to update prices.");
            }
            else if (response.StatusCode ==
                     HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    await ReadErrorMessageAsync(
                        response,
                        "The price could not be updated.",
                        cancellationToken));
            }
            else
            {
                TempData["SuccessMessage"] =
                    "The price was updated successfully.";

                return RedirectToPage("/Prices/Index");
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Updating price {PriceId} failed.",
                Id);

            ModelState.AddModelError(
                string.Empty,
                "The Price service is currently unavailable.");
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Updating price {PriceId} timed out.",
                Id);

            ModelState.AddModelError(
                string.Empty,
                "The Price service took too long to respond.");
        }

        if (Input.ProductId.HasValue)
        {
            await LoadProductNameAsync(
                client,
                Input.ProductId.Value,
                cancellationToken);
        }

        return Page();
    }

    private async Task LoadProductNameAsync(
        HttpClient client,
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(
                $"api/products/{productId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                ProductName = $"Product {productId}";
                return;
            }

            var product =
                await response.Content.ReadFromJsonAsync<
                    ProductResponse>(
                    cancellationToken: cancellationToken);

            ProductName =
                product?.Name ?? $"Product {productId}";
        }
        catch (HttpRequestException)
        {
            ProductName = $"Product {productId}";
        }
    }

    private IActionResult RedirectToLogin()
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page(
                    "/Prices/Edit",
                    new { id = Id })
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