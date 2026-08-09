using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookLoverECommerce.Cms.Pages.Categories;

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
    public int Id { get; set; }

    [BindProperty]
    public CategoryFormModel Input { get; set; } = new();

    public bool IsActive { get; private set; }
    public bool IsRootCategory { get; private set; }

    public IReadOnlyList<SelectListItem> ParentCategoryOptions
    { get; private set; } = Array.Empty<SelectListItem>();

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            using var response = await client.GetAsync(
                $"api/categories/{Id}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] =
                    $"Category #{Id} could not be found.";

                return RedirectToPage("/Categories/Index");
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The category could not be loaded.");

                await LoadParentCategoriesAsync(cancellationToken);
                return Page();
            }

            var category =
                await response.Content.ReadFromJsonAsync<CategoryResponse>(
                    cancellationToken: cancellationToken);

            if (category is null)
            {
                return NotFound();
            }

            Input = new CategoryFormModel
            {
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                ParentCategoryId = category.ParentCategoryId
            };

            IsActive = category.IsActive;
            IsRootCategory = !category.ParentCategoryId.HasValue;

            await LoadParentCategoriesAsync(cancellationToken);

            return Page();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The category API request failed for category {CategoryId}.",
                Id);

            ModelState.AddModelError(
                string.Empty,
                "The Products service is currently unavailable.");

            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (Id <= 0)
        {
            return NotFound();
        }

        if (Input.ParentCategoryId == Id)
        {
            ModelState.AddModelError(
                "Input.ParentCategoryId",
                "A category cannot be its own parent.");
        }

        if (!ModelState.IsValid)
        {
            await LoadPageStateAsync(cancellationToken);
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
            using var response = await client.PutAsJsonAsync(
                $"api/categories/{Id}",
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
                    "Your account does not have permission to edit categories.");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] =
                    $"Category #{Id} could not be found.";

                return RedirectToPage("/Categories/Index");
            }
            else if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "The category could not be updated.",
                    cancellationToken);

                ModelState.AddModelError(string.Empty, message);
            }
            else
            {
                TempData["SuccessMessage"] =
                    $"Category “{Input.Name.Trim()}” was updated successfully.";

                return RedirectToPage("/Categories/Index");
            }
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The update-category API request failed for {CategoryId}.",
                Id);

            ModelState.AddModelError(
                string.Empty,
                "The Products service is currently unavailable.");
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The update-category API request timed out for {CategoryId}.",
                Id);

            ModelState.AddModelError(
                string.Empty,
                "The Products service took too long to respond.");
        }

        await LoadPageStateAsync(cancellationToken);
        return Page();
    }

    private async Task LoadPageStateAsync(
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            var category = await client.GetFromJsonAsync<CategoryResponse>(
                $"api/categories/{Id}",
                cancellationToken);

            IsActive = category?.IsActive ?? false;
            IsRootCategory =
                category is not null &&
                !category.ParentCategoryId.HasValue;
        }
        catch (HttpRequestException)
        {
            IsActive = false;
        }

        await LoadParentCategoriesAsync(cancellationToken);
    }

    private async Task LoadParentCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            var categories =
                await client.GetFromJsonAsync<List<CategoryResponse>>(
                    "api/categories",
                    cancellationToken)
                ?? [];

            ParentCategoryOptions = categories
            .Where(category =>
                category.Id != Id &&
                !category.ParentCategoryId.HasValue &&
                category.IsActive)
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
                "Parent categories could not be loaded.");

            ParentCategoryOptions =
                Array.Empty<SelectListItem>();
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
                    "/Categories/Edit",
                    values: new { id = Id })
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