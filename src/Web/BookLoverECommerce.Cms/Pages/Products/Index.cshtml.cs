using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages.Products;

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

    public IReadOnlyList<ProductResponse> Products { get; private set; } =
        Array.Empty<ProductResponse>();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            // This page is protected by AdminOnly, so always use the
            // administrator endpoint that returns products of all statuses.
            using var response = await client.GetAsync(
                "api/products/admin",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToLogin();
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Your account does not have permission to view products.";

                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = await ReadErrorMessageAsync(
                    response,
                    "Products could not be loaded.",
                    cancellationToken);

                return Page();
            }

            Products =
                await response.Content.ReadFromJsonAsync<
                    List<ProductResponse>>(
                    cancellationToken: cancellationToken)
                ?? [];
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "The Products API request failed.");

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The Products API request timed out.");

            ErrorMessage =
                "The Products service took too long to respond.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await SendProductActionAsync(
            id,
            HttpMethod.Patch,
            $"api/products/{id}/publish",
            $"Product #{id} was published successfully.",
            "The product could not be published.",
            cancellationToken);
    }

    public async Task<IActionResult> OnPostArchiveAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await SendProductActionAsync(
            id,
            HttpMethod.Patch,
            $"api/products/{id}/archive",
            $"Product #{id} was archived successfully.",
            "The product could not be archived.",
            cancellationToken);
    }

    public async Task<IActionResult> OnPostUnarchiveAsync(
    int id,
    CancellationToken cancellationToken)
    {
        return await SendProductActionAsync(
            id,
            HttpMethod.Patch,
            $"api/products/{id}/unarchive",
            $"Product #{id} was restored to draft successfully.",
            "The product could not be restored.",
            cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await SendProductActionAsync(
            id,
            HttpMethod.Delete,
            $"api/products/{id}",
            $"Product #{id} was deleted successfully.",
            "The product could not be deleted.",
            cancellationToken);
    }

    private async Task<IActionResult> SendProductActionAsync(
        int id,
        HttpMethod method,
        string requestUri,
        string successMessage,
        string defaultErrorMessage,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            ErrorMessage = "A valid product ID is required.";
            return RedirectToPage();
        }

        var client = _httpClientFactory.CreateClient("ApiGateway");

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
                "Product action failed for product {ProductId}.",
                id);

            ErrorMessage =
                "The Products service is currently unavailable.";
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Product action timed out for product {ProductId}.",
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
                returnUrl = Url.Page("/Products/Index")
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
                await response.Content.ReadFromJsonAsync<ApiErrorResponse>(
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

    private sealed record ApiErrorResponse(string? Message);
}