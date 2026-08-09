using BookLoverECommerce.Price.Application.Abstractions;
using BookLoverECommerce.Price.Application.DTOs;
using BookLoverECommerce.Price.Application.Exceptions;
using BookLoverECommerce.Price.Domain.Entities;

namespace BookLoverECommerce.Price.Application.Prices;

public sealed class ProductPriceService : IProductPriceService
{
    private readonly IProductPriceRepository _repository;

    public ProductPriceService(IProductPriceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<ProductPriceDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var prices = await _repository.GetAllAsync(cancellationToken);

        return prices
            .Select(MapToDto)
            .ToArray();
    }

    public async Task<ProductPriceDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var productPrice = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (productPrice is null)
        {
            throw new ProductPriceNotFoundException(id);
        }

        return MapToDto(productPrice);
    }

    public async Task<ProductPriceDto> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var productPrice = await _repository.GetByProductIdAsync(
            productId,
            cancellationToken);

        if (productPrice is null)
        {
            throw new ProductPriceForProductNotFoundException(productId);
        }

        return MapToDto(productPrice);
    }

    public async Task<ProductPriceDto> CreateAsync(
        CreateProductPriceCommand command,
        CancellationToken cancellationToken = default)
    {
        var alreadyExists = await _repository.ExistsForProductAsync(
            command.ProductId,
            cancellationToken);

        if (alreadyExists)
        {
            throw new DuplicateProductPriceException(command.ProductId);
        }

        var productPrice = new ProductPrice(
            command.ProductId,
            command.BasePrice,
            command.Currency,
            command.SalePrice,
            NormalizeUtc(command.SaleStartDate),
            NormalizeUtc(command.SaleEndDate));

        await _repository.AddAsync(
            productPrice,
            cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(productPrice);
    }

    public async Task<ProductPriceDto> UpdateAsync(
        Guid id,
        UpdateProductPriceCommand command,
        CancellationToken cancellationToken = default)
    {
        var productPrice = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (productPrice is null)
        {
            throw new ProductPriceNotFoundException(id);
        }

        productPrice.Update(
            command.BasePrice,
            command.Currency,
            command.SalePrice,
            NormalizeUtc(command.SaleStartDate),
            NormalizeUtc(command.SaleEndDate));

        await _repository.SaveChangesAsync(cancellationToken);

        return MapToDto(productPrice);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var productPrice = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (productPrice is null)
        {
            throw new ProductPriceNotFoundException(id);
        }

        _repository.Remove(productPrice);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static ProductPriceDto MapToDto(ProductPrice productPrice)
    {
        var now = DateTime.UtcNow;

        return new ProductPriceDto(
            productPrice.Id,
            productPrice.ProductId,
            productPrice.BasePrice,
            productPrice.SalePrice,
            productPrice.GetEffectivePrice(now),
            productPrice.Currency.ToString(),
            productPrice.IsSaleActive(now),
            productPrice.SaleStartDate,
            productPrice.SaleEndDate,
            productPrice.CreatedAt,
            productPrice.UpdatedAt);
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }
}
