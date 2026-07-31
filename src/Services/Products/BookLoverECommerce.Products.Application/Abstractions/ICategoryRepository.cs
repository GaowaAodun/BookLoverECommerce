namespace BookLoverECommerce.Products.Application.Abstractions;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);
}