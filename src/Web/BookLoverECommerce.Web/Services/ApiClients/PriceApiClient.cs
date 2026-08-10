using System.Net.Http.Json;
using BookLoverECommerce.Web.Models.Prices;
using BookLoverECommerce.Web.Services.Interfaces;

namespace BookLoverECommerce.Web.Services.ApiClients;

public sealed class PriceApiClient : IPriceApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PriceApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory =
            httpClientFactory;
    }

    public async Task<PriceQuoteResponse> GetQuoteAsync(
        PriceQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient(
                "GatewayAuthorized");

        using var response =
            await client.PostAsJsonAsync(
                "/api/prices/quote",
                request,
                cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Price quote failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }

        var quote =
            await response.Content.ReadFromJsonAsync<
                PriceQuoteResponse>(
                cancellationToken:
                    cancellationToken);

        return quote
            ?? throw new InvalidOperationException(
                "Price API returned an empty response.");
    }
}