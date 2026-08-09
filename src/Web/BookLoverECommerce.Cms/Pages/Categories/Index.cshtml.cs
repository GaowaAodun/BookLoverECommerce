using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages.Categories;

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

    public IReadOnlyList<CategoryResponse> Categories
        { get; private set; } = Array.Empty<CategoryResponse>();

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
            using var response = await client.GetAsync(
                "api/categories/admin",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account does not have permission to view categories.";

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

            Categories =
                await response.Content.ReadFromJsonAsync<
                    List<CategoryResponse>>(
                    cancellationToken: cancellationToken)
                ?? [];
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

        return Page();
    }

    public Task<IActionResult> OnPostActivateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return SendCategoryActionAsync(
            id,
            HttpMethod.Patch,
            $"api/categories/{id}/activate",
            $"Category #{id} was activated successfully.",
            "The category could not be activated.",
            cancellationToken);
    }

    public Task<IActionResult> OnPostDeactivateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return SendCategoryActionAsync(
            id,
            HttpMethod.Patch,
            $"api/categories/{id}/deactivate",
            $"Category #{id} was deactivated successfully.",
            "The category could not be deactivated.",
            cancellationToken);
    }

    public Task<IActionResult> OnPostDeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return SendCategoryActionAsync(
            id,
            HttpMethod.Delete,
            $"api/categories/{id}",
            $"Category #{id} was deleted successfully.",
            "The category could not be deleted.",
            cancellationToken);
    }

    private async Task<IActionResult> SendCategoryActionAsync(
        int id,
        HttpMethod method,
        string requestUri,
        string successMessage,
        string defaultErrorMessage,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            ErrorMessage = "A valid category ID is required.";
            return RedirectToPage();
        }

        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        using var request = new HttpRequestMessage(
            method,
            requestUri);

        try
        {
            using var response = await client.SendAsync(
                request,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account does not have permission to perform this action.";

                return RedirectToPage();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    defaultErrorMessage,
                    cancellationToken);

                return RedirectToPage();
            }

            SuccessMessage = successMessage;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Category action failed for category {CategoryId}.",
                id);

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Category action timed out for category {CategoryId}.",
                id);

            ErrorMessage =
                "The Products service took too long to respond.";
        }

        return RedirectToPage();
    }

    private IActionResult RedirectToLogin()
    {
        HttpContext.Session.Remove("AccessToken");

        return RedirectToPage(
            "/Account/Login",
            new
            {
                returnUrl = Url.Page("/Categories/Index")
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