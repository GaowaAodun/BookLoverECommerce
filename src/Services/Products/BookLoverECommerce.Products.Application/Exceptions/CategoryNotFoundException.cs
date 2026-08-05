namespace BookLoverECommerce.Products.Application.Exceptions;

public sealed class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(int categoryId)
        : base($"Category with ID {categoryId} was not found.")
    {
    }
}