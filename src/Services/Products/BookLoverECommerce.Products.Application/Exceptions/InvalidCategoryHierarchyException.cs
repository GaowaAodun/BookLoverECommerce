namespace BookLoverECommerce.Products.Application.Exceptions;

public sealed class InvalidCategoryHierarchyException : Exception
{
    public InvalidCategoryHierarchyException(string message)
        : base(message)
    {
    }
}