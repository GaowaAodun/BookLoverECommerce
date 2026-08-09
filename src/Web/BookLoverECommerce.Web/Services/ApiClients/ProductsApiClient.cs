using System.Net.Http.Json;
using BookLoverECommerce.Web.Models.Products;
using BookLoverECommerce.Web.Services.Interfaces;

namespace BookLoverECommerce.Web.Services.ApiClients;

public sealed class ProductsApiClient : IProductsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductsApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ProductViewModel>> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("GatewayAuthorized");

        using var response = await client.GetAsync(
            "/api/products",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var products =
            await response.Content
                .ReadFromJsonAsync<List<ProductViewModel>>(
                    cancellationToken: cancellationToken);

        return products ?? [];
    }
}