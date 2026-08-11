namespace BookLoverECommerce.Web.Models.Orders;

public sealed class CreateOrderRequest
{
    public IReadOnlyCollection<CreateOrderItemRequest> Items
        { get; set; } =
        Array.Empty<CreateOrderItemRequest>();

    public ShippingAddressRequest ShippingAddress
        { get; set; } =
        new();
}