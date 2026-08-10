using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Order.Application.DTOs.External;
using BookLoverECommerce.Order.Application.Interfaces;

namespace BookLoverECommerce.Order.Infrastructure.Clients;

public sealed class ProductCatalogClient : IProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public ProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductSnapshot?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/products/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductSnapshot>(
                cancellationToken: cancellationToken);
    }
}