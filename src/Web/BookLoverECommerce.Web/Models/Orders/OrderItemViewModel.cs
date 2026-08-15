namespace BookLoverECommerce.Web.Models.Orders;

public sealed class OrderItemViewModel
{
    // Returned by Order API
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } =
        string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }


    // =========================================
    // FRONTEND COMPATIBILITY PROPERTIES
    // =========================================

    // Existing Razor page expects LineTotal.
    public decimal LineTotal =>
        Subtotal;

    // Order API currently does not return an image.
    // We can enrich this later from Products API.
    public string? ImageUrl { get; set; }
}