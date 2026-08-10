namespace BookLoverECommerce.Web.Models.Prices;

public sealed class PriceQuoteResponse
{
    public List<PriceQuoteItemResponse> Items { get; set; } =
        new();

    public decimal Subtotal { get; set; }

    public string Currency { get; set; } =
        "CAD";
}