using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookLoverECommerce.Cms.Pages.Products;

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
    public CreateProductRequest Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> Categories { get; private set; } =
        Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> ProductTypes { get; private set; } =
        Array.Empty<SelectListItem>();

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        PopulateProductTypes();

        var result = await LoadCategoriesAsync(cancellationToken);

        return result ?? Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        PopulateProductTypes();

        var categoryResult =
            await LoadCategoriesAsync(cancellationToken);

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
        Input.Sku = Input.Sku.Trim();
        Input.Brand = NormalizeOptionalValue(Input.Brand);
        Input.ThumbnailUrl =
            NormalizeOptionalValue(Input.ThumbnailUrl);

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/products",
                Input,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account does not have permission to create products.";

                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    response.StatusCode == HttpStatusCode.Conflict
                        ? "A product with this SKU already exists."
                        : "The product could not be created.",
                    cancellationToken);

                return Page();
            }

            var product =
                await response.Content.ReadFromJsonAsync<ProductResponse>(
                    cancellationToken: cancellationToken);

            TempData["SuccessMessage"] = product is null
                ? "The product was created successfully."
                : $"{product.Name} was created successfully.";

            return RedirectToPage("/Products/Index");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The create product request failed.");

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The create product request timed out.");

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
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account cannot access product categories.";

                return Page();
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
                    GetCategoryLabel(category, categories),
                    category.Id.ToString()))
                .ToArray();

            if (Categories.Count == 0)
            {
                ErrorMessage =
                    "No active categories are available. Create or activate a category before adding a product.";
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The Categories API request failed.");

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The Categories API request timed out.");

            ErrorMessage =
                "The Products service took too long to respond.";
        }

        return null;
    }

    private void PopulateProductTypes()
    {
        ProductTypes = Enum
            .GetValues<ProductType>()
            .Select(productType => new SelectListItem(
                GetProductTypeLabel(productType),
                ((int)productType).ToString()))
            .ToArray();
    }

    private IActionResult RedirectToLogin()
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page("/Products/Create")
            });
    }

    private static string GetCategoryLabel(
        CategoryResponse category,
        IReadOnlyCollection<CategoryResponse> categories)
    {
        if (category.ParentCategoryId is null)
        {
            return category.Name;
        }

        var parent = categories.FirstOrDefault(
            item => item.Id == category.ParentCategoryId);

        return parent is null
            ? category.Name
            : $"{parent.Name} — {category.Name}";
    }

    private static string GetProductTypeLabel(
        ProductType productType)
    {
        return productType switch
        {
            ProductType.PrintedBook => "Printed Book",
            ProductType.HomeAndKitchen => "Home & Kitchen",
            _ => productType.ToString()
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