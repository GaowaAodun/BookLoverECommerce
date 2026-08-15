namespace BookLoverECommerce.Web.Models.Orders;

public sealed class CreateOrderItemRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}