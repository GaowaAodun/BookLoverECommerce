namespace BookLoverECommerce.Price.Application.Exceptions;

public sealed class ProductPriceForProductNotFoundException : Exception
{
    public ProductPriceForProductNotFoundException(Guid productId)
        : base($"Price for product '{productId}' was not found.")
    {
    }
}
