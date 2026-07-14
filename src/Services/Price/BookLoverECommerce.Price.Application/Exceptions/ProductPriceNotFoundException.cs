namespace BookLoverECommerce.Price.Application.Exceptions;

public sealed class ProductPriceNotFoundException : Exception
{
    public ProductPriceNotFoundException(Guid id)
        : base($"Product price with ID '{id}' was not found.")
    {
    }
}
