using System.Net.Http.Json;
using BookLoverECommerce.Web.Models.Orders;
using BookLoverECommerce.Web.Services.Interfaces;

namespace BookLoverECommerce.Web.Services.ApiClients;

public sealed class OrderApiClient : IOrderApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderApiClient> _logger;

    public OrderApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<OrderApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


    // =========================================
    // CREATE ORDER
    // POST /api/orders
    // =========================================

    public async Task<OrderViewModel> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient(
                "GatewayAuthorized");

        using var response =
            await client.PostAsJsonAsync(
                "/api/orders",
                request,
                cancellationToken);

        await EnsureSuccessAsync(
            response,
            "Create order",
            cancellationToken);

        var order =
            await response.Content
                .ReadFromJsonAsync<OrderViewModel>(
                    cancellationToken:
                        cancellationToken);

        return order
            ?? throw new InvalidOperationException(
                "Order service returned an empty response.");
    }


    // =========================================
    // GET CURRENT CUSTOMER ORDERS
    // GET /api/orders/mine
    // =========================================

    public async Task<IReadOnlyCollection<OrderViewModel>>
        GetMyOrdersAsync(
            CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient(
                "GatewayAuthorized");

        using var response =
            await client.GetAsync(
                "/api/orders/mine",
                cancellationToken);

        await EnsureSuccessAsync(
            response,
            "Get orders",
            cancellationToken);

        var orders =
            await response.Content
                .ReadFromJsonAsync<List<OrderViewModel>>(
                    cancellationToken:
                        cancellationToken);

        return orders
            ?? new List<OrderViewModel>();
    }


    // =========================================
    // COMPATIBILITY METHOD
    //
    // Existing Orders/Index.cshtml.cs uses this.
    // =========================================

    public async Task<IReadOnlyCollection<OrderViewModel>>
        GetOrdersAsync(
            CancellationToken cancellationToken = default)
    {
        return await GetMyOrdersAsync(
            cancellationToken);
    }


    // =========================================
    // GET ONE CUSTOMER ORDER
    //
    // Backend currently has no:
    // GET /api/orders/{id}
    //
    // Therefore retrieve /mine and select the order.
    // =========================================

    public async Task<OrderViewModel?> GetOrderByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var orders =
            await GetMyOrdersAsync(
                cancellationToken);

        return orders.FirstOrDefault(
            order => order.Id == orderId);
    }


    // =========================================
    // CANCEL ORDER
    // POST /api/orders/{id}/cancel
    // =========================================

    public async Task CancelOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient(
                "GatewayAuthorized");

        using var response =
            await client.PostAsync(
                $"/api/orders/{orderId}/cancel",
                content: null,
                cancellationToken);

        await EnsureSuccessAsync(
            response,
            "Cancel order",
            cancellationToken);
    }


    // =========================================
    // CONFIRM RECEIPT
    // POST /api/orders/{id}/confirm-receipt
    // =========================================

    public async Task ConfirmReceiptAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient(
                "GatewayAuthorized");

        using var response =
            await client.PostAsync(
                $"/api/orders/{orderId}/confirm-receipt",
                content: null,
                cancellationToken);

        await EnsureSuccessAsync(
            response,
            "Confirm receipt",
            cancellationToken);
    }


    // =========================================
    // ERROR HANDLING
    // =========================================

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body =
            await response.Content
                .ReadAsStringAsync(
                    cancellationToken);

        _logger.LogWarning(
            "{Operation} failed. Status: {Status}. Response: {Response}",
            operation,
            response.StatusCode,
            body);

        throw new HttpRequestException(
            $"{operation} failed. " +
            $"Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. " +
            $"Response: {body}");
    }
}