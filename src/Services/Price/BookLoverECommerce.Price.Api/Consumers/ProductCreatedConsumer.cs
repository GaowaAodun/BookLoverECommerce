using BookLoverECommerce.Contracts.Products;
using MassTransit;

namespace BookLoverECommerce.Price.Api.Consumers;

public sealed class ProductCreatedConsumer(
    ILogger<ProductCreatedConsumer> logger)
    : IConsumer<ProductCreated>
{
    public Task Consume(
        ConsumeContext<ProductCreated> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received ProductCreated: ID={ProductId}, SKU={Sku}, Price={Price}",
            message.ProductId,
            message.Sku,
            message.Price);

        return Task.CompletedTask;
    }
}