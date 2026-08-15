namespace BookLoverECommerce.Web.Models.Prices;

public sealed class PriceQuoteItemResponse
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public string Currency { get; set; } =
        string.Empty;

    public bool IsSaleActive { get; set; }
}