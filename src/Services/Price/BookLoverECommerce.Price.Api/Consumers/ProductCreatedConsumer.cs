using BookLoverECommerce.Contracts.Products;
using BookLoverECommerce.Price.Application.DTOs;
using BookLoverECommerce.Price.Application.Exceptions;
using BookLoverECommerce.Price.Application.Prices;
using BookLoverECommerce.Price.Domain.Enums;
using MassTransit;

namespace BookLoverECommerce.Price.Api.Consumers;

public sealed class ProductCreatedConsumer
    : IConsumer<ProductCreated>
{
    private readonly IProductPriceService _productPriceService;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(
        IProductPriceService productPriceService,
        ILogger<ProductCreatedConsumer> logger)
    {
        _productPriceService = productPriceService;
        _logger = logger;
    }

    public async Task Consume(
        ConsumeContext<ProductCreated> context)
    {
        var message = context.Message;

        try
        {
            await _productPriceService.CreateAsync(
                new CreateProductPriceCommand(
                    ProductId: message.ProductId,
                    BasePrice: message.Price,
                    Currency: Currency.CAD,
                    SalePrice: null,
                    SaleStartDate: null,
                    SaleEndDate: null),
                context.CancellationToken);

            _logger.LogInformation(
                "Initial price created for ProductId={ProductId}, SKU={Sku}, Price={Price} {Currency}",
                message.ProductId,
                message.Sku,
                message.Price,
                Currency.CAD);
        }
        catch (DuplicateProductPriceException)
        {
            _logger.LogInformation(
                "Price already exists for ProductId={ProductId}. Duplicate ProductCreated event ignored.",
                message.ProductId);
        }
    }
}