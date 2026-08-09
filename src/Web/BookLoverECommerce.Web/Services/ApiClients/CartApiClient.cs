using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Web.Models.Cart;
using BookLoverECommerce.Web.Services.Interfaces;

namespace BookLoverECommerce.Web.Services.ApiClients;

public sealed class CartApiClient : ICartApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CartApiClient> _logger;

    public CartApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<CartApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<CartViewModel?> GetCartAsync(
    string userId,
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient(
            "GatewayAuthorized");

    using var response =
        await client.GetAsync(
            $"/api/cart/{userId}",
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Get cart failed. " +
            $"Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. " +
            $"Response: {responseBody}");
    }

    return System.Text.Json.JsonSerializer
        .Deserialize<CartViewModel>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
}

    public async Task<CartViewModel?> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("GatewayAuthorized");

        using var response =
            await client.PostAsJsonAsync(
                $"/api/cart/{userId}/items",
                request,
                cancellationToken);

        await EnsureSuccessAsync(
            response,
            "Add cart item",
            cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<CartViewModel>(
                cancellationToken: cancellationToken);
    }

    public async Task<CartViewModel?> UpdateItemAsync(
        string userId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("GatewayAuthorized");

        using var response =
            await client.PutAsJsonAsync(
                $"/api/cart/{userId}/items",
                request,
                cancellationToken);

        await EnsureSuccessAsync(
            response,
            "Update cart item",
            cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<CartViewModel>(
                cancellationToken: cancellationToken);
    }

    public async Task<CartViewModel?> RemoveItemAsync(
        string userId,
        RemoveCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("GatewayAuthorized");

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/cart/{userId}/items")
            {
                Content = JsonContent.Create(request)
            };

        using var response =
            await client.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(
            response,
            "Remove cart item",
            cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<CartViewModel>(
                cancellationToken: cancellationToken);
    }

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
            await response.Content.ReadAsStringAsync(
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