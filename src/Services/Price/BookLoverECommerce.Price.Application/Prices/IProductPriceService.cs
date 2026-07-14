using BookLoverECommerce.Price.Application.DTOs;

namespace BookLoverECommerce.Price.Application.Prices;

public interface IProductPriceService
{
    Task<IReadOnlyCollection<ProductPriceDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ProductPriceDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ProductPriceDto> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<ProductPriceDto> CreateAsync(
        CreateProductPriceCommand command,
        CancellationToken cancellationToken = default);

    Task<ProductPriceDto> UpdateAsync(
        Guid id,
        UpdateProductPriceCommand command,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
