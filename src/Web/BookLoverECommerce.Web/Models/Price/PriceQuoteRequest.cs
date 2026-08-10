namespace BookLoverECommerce.Web.Models.Prices;

public sealed class PriceQuoteRequest
{
    public List<PriceQuoteItemRequest> Items { get; set; } =
        new();
}