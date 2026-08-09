namespace BookLoverECommerce.Products.Application.Exceptions;

public sealed class CategoryInUseException : Exception
{
    public CategoryInUseException(string message)
        : base(message)
    {
    }
}