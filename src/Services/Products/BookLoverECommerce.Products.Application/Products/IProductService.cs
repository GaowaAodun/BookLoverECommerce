using BookLoverECommerce.Products.Application.DTOs;

namespace BookLoverECommerce.Products.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        IReadOnlyCollection<Guid>? productIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default);

    Task<ProductDto> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<ProductDto> UpdateAsync(
        Guid productId,
        UpdateProductCommand command,
        CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task ArchiveAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task UnarchiveAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}