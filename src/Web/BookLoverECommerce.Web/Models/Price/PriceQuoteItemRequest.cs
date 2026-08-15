namespace BookLoverECommerce.Web.Models.Prices;

public sealed class PriceQuoteItemRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}