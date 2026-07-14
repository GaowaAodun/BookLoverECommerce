namespace BookLoverECommerce.Price.Application.Exceptions;

public sealed class DuplicateProductPriceException : Exception
{
    public DuplicateProductPriceException(Guid productId)
        : base($"A price already exists for product '{productId}'.")
    {
    }
}
