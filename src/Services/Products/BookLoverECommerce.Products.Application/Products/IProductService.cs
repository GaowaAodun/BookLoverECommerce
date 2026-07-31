using BookLoverECommerce.Products.Application.DTOs;

namespace BookLoverECommerce.Products.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        IReadOnlyCollection<int>? productIds,
        CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task ArchiveAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        int productId,
        CancellationToken cancellationToken = default);
}