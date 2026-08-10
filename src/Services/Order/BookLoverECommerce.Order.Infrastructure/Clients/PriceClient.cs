using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Order.Application.DTOs;
using BookLoverECommerce.Order.Application.Interfaces;

namespace BookLoverECommerce.Order.Infrastructure.Clients;

public sealed class PriceClient : IPriceClient
{
    private readonly HttpClient _httpClient;

    public PriceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PriceSnapshotResponse?> GetPriceAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid product ID is required.",
                nameof(productId));
        }

        var requestUri =
            $"api/prices/product/{productId:D}";

        using var response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<PriceSnapshotResponse>(
                cancellationToken: cancellationToken);
    }
}