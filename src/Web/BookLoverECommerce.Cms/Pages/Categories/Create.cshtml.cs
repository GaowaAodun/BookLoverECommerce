using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookLoverECommerce.Cms.Pages.Categories;

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
    public CategoryFormModel Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> ParentCategoryOptions
        { get; private set; } = Array.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        return await LoadParentCategoriesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadParentCategoriesAsync(cancellationToken);
            return Page();
        }

        var command = new
        {
            Name = Input.Name.Trim(),
            Description = NormalizeOptionalText(Input.Description),
            Input.DisplayOrder,
            Input.ParentCategoryId
        };

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/categories",
                command,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account does not have permission to create categories.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "The category could not be created.",
                    cancellationToken);

                ModelState.AddModelError(string.Empty, message);
            }
            else
            {
                TempData["SuccessMessage"] =
                    $"Category “{Input.Name.Trim()}” was created successfully.";

                return RedirectToPage("/Categories/Index");
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The create-category API request failed.");

            ModelState.AddModelError(
                string.Empty,
                "The Products service is currently unavailable.");
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The create-category API request timed out.");

            ModelState.AddModelError(
                string.Empty,
                "The Products service took too long to respond.");
        }

        await LoadParentCategoriesAsync(cancellationToken);
        return Page();
    }

    private async Task<IActionResult> LoadParentCategoriesAsync(
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

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Parent categories could not be loaded.");

                return Page();
            }

            var categories =
                await response.Content.ReadFromJsonAsync<
                    List<CategoryResponse>>(
                    cancellationToken: cancellationToken)
                ?? [];

            ParentCategoryOptions = categories
                .OrderBy(category => category.DisplayOrder)
                .ThenBy(category => category.Name)
                .Select(category => new SelectListItem(
                    category.Name,
                    category.Id.ToString()))
                .ToArray();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The parent-category API request failed.");

            ModelState.AddModelError(
                string.Empty,
                "Parent categories could not be loaded.");
        }

        return Page();
    }

    private IActionResult RedirectToLogin()
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page("/Categories/Create")
            });
    }

    private static string? NormalizeOptionalText(string? value)
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