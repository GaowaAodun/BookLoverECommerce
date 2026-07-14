namespace BookLoverECommerce.Products.Application.Exceptions;

public sealed class DuplicateSkuException : Exception
{
    public DuplicateSkuException(string sku)
        : base($"A product with SKU '{sku}' already exists.")
    {
    }
}