namespace BookLoverECommerce.Products.Application.Exceptions;

public sealed class DuplicateCategoryNameException : Exception
{
    public DuplicateCategoryNameException(string name)
        : base($"A category named '{name}' already exists.")
    {
    }
}