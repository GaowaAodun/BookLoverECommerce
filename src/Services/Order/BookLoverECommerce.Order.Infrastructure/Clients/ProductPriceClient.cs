using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Order.Application.DTOs.External;
using BookLoverECommerce.Order.Application.Interfaces;

namespace BookLoverECommerce.Order.Infrastructure.Clients;

public sealed class ProductPriceClient : IProductPriceClient
{
    private readonly HttpClient _httpClient;

    public ProductPriceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductPriceSnapshot?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/prices/product/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductPriceSnapshot>(
                cancellationToken: cancellationToken);
    }
}