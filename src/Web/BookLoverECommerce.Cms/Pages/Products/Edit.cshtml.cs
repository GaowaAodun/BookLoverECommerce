using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookLoverECommerce.Cms.Pages.Products;

[Authorize(Roles = "Admin")]
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

    [BindProperty]
    public UpdateProductRequest Input { get; set; } = new();

    public string Sku { get; private set; } = string.Empty;

    public IReadOnlyList<SelectListItem> Categories { get; private set; } =
        Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> ProductTypes { get; private set; } =
        Array.Empty<SelectListItem>();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        PopulateProductTypes();

        var categoryResult = await LoadCategoriesAsync(
            cancellationToken);

        if (categoryResult is not null)
        {
            return categoryResult;
        }

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.GetAsync(
                $"api/products/{id}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin(id);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    "The product could not be loaded.",
                    cancellationToken);

                return Page();
            }

            var product =
                await response.Content.ReadFromJsonAsync<ProductResponse>(
                    cancellationToken: cancellationToken);

            if (product is null)
            {
                ErrorMessage = "The Products API returned an empty response.";
                return Page();
            }

            Sku = product.Sku;

            Input = new UpdateProductRequest
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ProductType = product.ProductType,
                Brand = product.Brand,
                ThumbnailUrl = product.ThumbnailUrl
            };
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Failed to load product {ProductId}.",
                id);

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Loading product {ProductId} timed out.",
                id);

            ErrorMessage =
                "The Products service took too long to respond.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        PopulateProductTypes();

        var categoryResult = await LoadCategoriesAsync(
            cancellationToken);

        if (categoryResult is not null)
        {
            return categoryResult;
        }

        if (!Enum.IsDefined(Input.ProductType))
        {
            ModelState.AddModelError(
                "Input.ProductType",
                "Select a valid product type.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Input.Name = Input.Name.Trim();
        Input.Description = Input.Description.Trim();
        Input.Brand = NormalizeOptionalValue(Input.Brand);
        Input.ThumbnailUrl =
            NormalizeOptionalValue(Input.ThumbnailUrl);

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.PutAsJsonAsync(
                $"api/products/{id}",
                Input,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin(id);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    "The product could not be updated.",
                    cancellationToken);

                return Page();
            }

            TempData["SuccessMessage"] =
                $"{Input.Name} was updated successfully.";

            return RedirectToPage("/Products/Index");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Failed to update product {ProductId}.",
                id);

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Updating product {ProductId} timed out.",
                id);

            ErrorMessage =
                "The Products service took too long to respond.";
        }

        return Page();
    }

    private async Task<IActionResult?> LoadCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.GetAsync(
                "api/categories",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    "Categories could not be loaded.",
                    cancellationToken);

                return Page();
            }

            var categories =
                await response.Content.ReadFromJsonAsync<
                    List<CategoryResponse>>(
                    cancellationToken: cancellationToken)
                ?? [];

            Categories = categories
                .Select(category => new SelectListItem(
                    category.Name,
                    category.Id.ToString()))
                .ToArray();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The Categories API request failed.");

            ErrorMessage =
                "The Products service is currently unavailable.";
        }

        return null;
    }

    private void PopulateProductTypes()
    {
        ProductTypes = Enum
            .GetValues<ProductType>()
            .Select(type => new SelectListItem(
                GetProductTypeLabel(type),
                ((int)type).ToString()))
            .ToArray();
    }

    private IActionResult RedirectToLogin(int id)
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page(
                    "/Products/Edit",
                    values: new { id })
            });
    }

    private static string GetProductTypeLabel(ProductType type)
    {
        return type switch
        {
            ProductType.PrintedBook => "Printed Book",
            ProductType.HomeAndKitchen => "Home & Kitchen",
            _ => type.ToString()
        };
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var error =
                await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
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