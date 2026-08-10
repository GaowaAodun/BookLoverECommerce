using System.Net.Http.Json;
using BookLoverECommerce.Order.Application.DTOs;
using BookLoverECommerce.Order.Application.Interfaces;

namespace BookLoverECommerce.Order.Infrastructure.Clients;

public sealed class ProductsClient : IProductsClient
{
    private readonly HttpClient _httpClient;

    public ProductsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductSnapshotResponse?> GetProductAsync(
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
            $"api/products?productIds={productId:D}";

        var products = await _httpClient
            .GetFromJsonAsync<IReadOnlyList<ProductSnapshotResponse>>(
                requestUri,
                cancellationToken);

        return products?.FirstOrDefault(
            product => product.Id == productId);
    }
}