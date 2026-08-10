using BookLoverECommerce.Price.Application.Abstractions;
using BookLoverECommerce.Price.Application.DTOs;
using BookLoverECommerce.Price.Application.Exceptions;
using BookLoverECommerce.Price.Domain.Entities;

namespace BookLoverECommerce.Price.Application.Prices;

public sealed class ProductPriceService : IProductPriceService
{
    private readonly IProductPriceRepository _repository;

    public ProductPriceService(
        IProductPriceRepository repository)
    {
        _repository = repository;
    }

    // =========================================
    // GET ALL
    // =========================================

    public async Task<IReadOnlyCollection<ProductPriceDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var prices =
            await _repository.GetAllAsync(
                cancellationToken);

        return prices
            .Select(MapToDto)
            .ToArray();
    }


    // =========================================
    // GET BY PRICE ID
    // =========================================

    public async Task<ProductPriceDto> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var productPrice =
            await _repository.GetByIdAsync(
                id,
                cancellationToken);

        if (productPrice is null)
        {
            throw new ProductPriceNotFoundException(
                id);
        }

        return MapToDto(
            productPrice);
    }


    // =========================================
    // GET BY PRODUCT ID
    // =========================================

    public async Task<ProductPriceDto> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var productPrice =
            await _repository.GetByProductIdAsync(
                productId,
                cancellationToken);

        if (productPrice is null)
        {
            throw new ProductPriceForProductNotFoundException(
                productId);
        }

        return MapToDto(
            productPrice);
    }


    // =========================================
    // GET CHECKOUT PRICE QUOTE
    // =========================================

    public async Task<PriceQuoteResponse> GetQuoteAsync(
        PriceQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items is null ||
            request.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one product is required.");
        }

        var response =
            new PriceQuoteResponse();

        string? quoteCurrency = null;

        var now =
            DateTime.UtcNow;


        foreach (var requestedItem in request.Items)
        {
            if (requestedItem.ProductId == Guid.Empty)
            {
                throw new ArgumentException(
                    "ProductId is required.");
            }

            if (requestedItem.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }


            var productPrice =
                await _repository.GetByProductIdAsync(
                    requestedItem.ProductId,
                    cancellationToken);


            if (productPrice is null)
            {
                throw new ProductPriceForProductNotFoundException(
                    requestedItem.ProductId);
            }


            var unitPrice =
                productPrice.GetEffectivePrice(
                    now);


            var currency =
                productPrice.Currency.ToString();


            quoteCurrency ??=
                currency;


            if (!string.Equals(
                    quoteCurrency,
                    currency,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "All checkout products must use the same currency.");
            }


            var lineTotal =
                unitPrice *
                requestedItem.Quantity;


            response.Items.Add(
                new PriceQuoteItemResponse
                {
                    ProductId =
                        requestedItem.ProductId,

                    Quantity =
                        requestedItem.Quantity,

                    UnitPrice =
                        unitPrice,

                    LineTotal =
                        lineTotal,

                    Currency =
                        currency,

                    IsSaleActive =
                        productPrice.IsSaleActive(
                            now)
                });
        }


        response.Subtotal =
            response.Items.Sum(
                item => item.LineTotal);


        response.Currency =
            quoteCurrency ?? "CAD";


        return response;
    }


    // =========================================
    // CREATE
    // =========================================

    public async Task<ProductPriceDto> CreateAsync(
        CreateProductPriceCommand command,
        CancellationToken cancellationToken = default)
    {
        var alreadyExists =
            await _repository.ExistsForProductAsync(
                command.ProductId,
                cancellationToken);


        if (alreadyExists)
        {
            throw new DuplicateProductPriceException(
                command.ProductId);
        }


        var productPrice =
            new ProductPrice(
                command.ProductId,
                command.BasePrice,
                command.Currency,
                command.SalePrice,
                NormalizeUtc(
                    command.SaleStartDate),
                NormalizeUtc(
                    command.SaleEndDate));


        await _repository.AddAsync(
            productPrice,
            cancellationToken);


        await _repository.SaveChangesAsync(
            cancellationToken);


        return MapToDto(
            productPrice);
    }


    // =========================================
    // UPDATE
    // =========================================

    public async Task<ProductPriceDto> UpdateAsync(
        Guid id,
        UpdateProductPriceCommand command,
        CancellationToken cancellationToken = default)
    {
        var productPrice =
            await _repository.GetByIdAsync(
                id,
                cancellationToken);


        if (productPrice is null)
        {
            throw new ProductPriceNotFoundException(
                id);
        }


        productPrice.Update(
            command.BasePrice,
            command.Currency,
            command.SalePrice,
            NormalizeUtc(
                command.SaleStartDate),
            NormalizeUtc(
                command.SaleEndDate));


        await _repository.SaveChangesAsync(
            cancellationToken);


        return MapToDto(
            productPrice);
    }


    // =========================================
    // DELETE
    // =========================================

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var productPrice =
            await _repository.GetByIdAsync(
                id,
                cancellationToken);


        if (productPrice is null)
        {
            throw new ProductPriceNotFoundException(
                id);
        }


        _repository.Remove(
            productPrice);


        await _repository.SaveChangesAsync(
            cancellationToken);
    }


    // =========================================
    // MAP ENTITY TO DTO
    // =========================================

    private static ProductPriceDto MapToDto(
        ProductPrice productPrice)
    {
        var now =
            DateTime.UtcNow;


        return new ProductPriceDto(
            productPrice.Id,
            productPrice.ProductId,
            productPrice.BasePrice,
            productPrice.SalePrice,
            productPrice.GetEffectivePrice(
                now),
            productPrice.Currency.ToString(),
            productPrice.IsSaleActive(
                now),
            productPrice.SaleStartDate,
            productPrice.SaleEndDate,
            productPrice.CreatedAt,
            productPrice.UpdatedAt);
    }


    // =========================================
    // NORMALIZE DATE TO UTC
    // =========================================

    private static DateTime? NormalizeUtc(
        DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }


        return value.Value.Kind switch
        {
            DateTimeKind.Utc =>
                value.Value,

            DateTimeKind.Local =>
                value.Value.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    value.Value,
                    DateTimeKind.Utc)
        };
    }
}