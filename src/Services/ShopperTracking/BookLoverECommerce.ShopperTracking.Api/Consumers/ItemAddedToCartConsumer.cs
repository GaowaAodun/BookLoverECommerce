using System.Text.Json;
using BookLoverECommerce.Contracts.Tracking;
using BookLoverECommerce.ShopperTracking.Domain.Entities;
using BookLoverECommerce.ShopperTracking.Infrastructure.Persistence;
using MassTransit;

namespace BookLoverECommerce.ShopperTracking.Api.Consumers;

public sealed class ItemAddedToCartConsumer
    : IConsumer<ItemAddedToCart>
{
    private readonly ShopperTrackingDbContext _dbContext;

    public ItemAddedToCartConsumer(
        ShopperTrackingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(
        ConsumeContext<ItemAddedToCart> context)
    {
        var message = context.Message;

        var shopperEvent =
            new ShopperEvent
            {
                CustomerId =
                    message.CustomerId,

                EventType =
                    "ItemAddedToCart",

                ProductId =
                    message.ProductId,

                Quantity =
                    message.Quantity,

                OccurredAt =
                    message.OccurredAt.UtcDateTime,

                Metadata =
                    JsonSerializer.Serialize(
                        new
                        {
                            message.ProductId,
                            message.Quantity
                        })
            };

        await _dbContext.ShopperEvents.AddAsync(
            shopperEvent,
            context.CancellationToken);

        await _dbContext.SaveChangesAsync(
            context.CancellationToken);
    }
}