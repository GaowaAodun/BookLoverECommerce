using BookLoverECommerce.Web.Models.Prices;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface IPriceApiClient
{
    Task<PriceQuoteResponse> GetQuoteAsync(
        PriceQuoteRequest request,
        CancellationToken cancellationToken = default);
}