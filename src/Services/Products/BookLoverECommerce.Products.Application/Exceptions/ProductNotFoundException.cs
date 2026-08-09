namespace BookLoverECommerce.Products.Application.Exceptions;

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Product with ID {productId} was not found.")
    {
    }
}