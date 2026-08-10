using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Cms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookLoverECommerce.Cms.Models.Orders;

namespace BookLoverECommerce.Cms.Pages.Orders;

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

    public IReadOnlyCollection<OrderViewModel> Orders { get; private set; }
        = Array.Empty<OrderViewModel>();

    [BindProperty]
    public PaymentSucceededViewModel PaymentInput { get; set; } = new();

    [BindProperty]
    public ShipOrderViewModel ShippingInput { get; set; } = new();

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
                "api/orders",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                HttpContext.Session.Remove("AccessToken");

                ErrorMessage =
                    "Your session has expired or you do not have permission.";

                return RedirectToPage("/Account/Login");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Loading orders failed with status code {StatusCode}.",
                    response.StatusCode);

                ErrorMessage =
                    "Orders could not be loaded. Please try again.";

                return Page();
            }

            Orders =
                await response.Content.ReadFromJsonAsync<
                    List<OrderViewModel>>(
                    cancellationToken: cancellationToken)
                ?? [];

            return Page();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Could not connect to the API Gateway while loading orders.");

            ErrorMessage =
                "The order service is currently unavailable.";

            return Page();
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The request for loading orders timed out.");

            ErrorMessage =
                "The order request timed out. Please try again.";

            return Page();
        }
    }

    public async Task<IActionResult> OnPostMarkPaidAsync(
        Guid orderId,
        string paymentReference,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(paymentReference))
        {
            ErrorMessage = "Payment reference is required.";
            return RedirectToPage();
        }

        var request = new
        {
            PaymentReference = paymentReference.Trim()
        };

        return await SendOrderActionAsync(
            orderId,
            "payment-succeeded",
            request,
            "The order was marked as paid.",
            cancellationToken);
    }

    public async Task<IActionResult> OnPostStartProcessingAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await SendOrderActionAsync(
            orderId,
            "start-processing",
            requestBody: null,
            "Order processing has started.",
            cancellationToken);
    }

    public async Task<IActionResult> OnPostShipAsync(
        Guid orderId,
        string carrier,
        string trackingNumber,
        string? trackingUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(carrier) ||
            string.IsNullOrWhiteSpace(trackingNumber))
        {
            ErrorMessage =
                "Carrier and tracking number are required.";

            return RedirectToPage();
        }

        var request = new
        {
            Carrier = carrier.Trim(),
            TrackingNumber = trackingNumber.Trim(),
            TrackingUrl = string.IsNullOrWhiteSpace(trackingUrl)
                ? null
                : trackingUrl.Trim()
        };

        return await SendOrderActionAsync(
            orderId,
            "ship",
            request,
            "The order was shipped successfully.",
            cancellationToken);
    }

    public async Task<IActionResult> OnPostCompleteAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await SendOrderActionAsync(
            orderId,
            "complete",
            requestBody: null,
            "The order was completed.",
            cancellationToken);
    }

    private async Task<IActionResult> SendOrderActionAsync(
        Guid orderId,
        string action,
        object? requestBody,
        string successMessage,
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient("ApiGateway");

        try
        {
            var endpoint =
                $"api/orders/{orderId}/{action}";

            using var response = requestBody is null
                ? await client.PostAsync(
                    endpoint,
                    content: null,
                    cancellationToken)
                : await client.PostAsJsonAsync(
                    endpoint,
                    requestBody,
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                HttpContext.Session.Remove("AccessToken");

                ErrorMessage =
                    "Your session has expired or you do not have permission.";

                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ErrorMessage = "The selected order was not found.";
                return RedirectToPage();
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                _logger.LogWarning(
                    "Order action {Action} failed for order {OrderId}. " +
                    "Status: {StatusCode}. Response: {ResponseBody}",
                    action,
                    orderId,
                    response.StatusCode,
                    responseBody);

                ErrorMessage =
                    "The order action could not be completed. " +
                    "Check the current order status and try again.";

                return RedirectToPage();
            }

            SuccessMessage = successMessage;
            return RedirectToPage();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Could not connect to the API Gateway while performing " +
                "action {Action} for order {OrderId}.",
                action,
                orderId);

            ErrorMessage =
                "The order service is currently unavailable.";

            return RedirectToPage();
        }
        catch (TaskCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "Order action {Action} timed out for order {OrderId}.",
                action,
                orderId);

            ErrorMessage =
                "The order request timed out. Please try again.";

            return RedirectToPage();
        }
    }
}