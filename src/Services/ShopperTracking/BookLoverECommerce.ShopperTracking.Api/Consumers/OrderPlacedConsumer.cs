using System.Text.Json;
using BookLoverECommerce.Contracts.Tracking;
using BookLoverECommerce.ShopperTracking.Domain.Entities;
using BookLoverECommerce.ShopperTracking.Infrastructure.Persistence;
using MassTransit;

namespace BookLoverECommerce.ShopperTracking.Api.Consumers;

public sealed class OrderPlacedConsumer
    : IConsumer<OrderPlaced>
{
    private readonly ShopperTrackingDbContext _dbContext;

    public OrderPlacedConsumer(
        ShopperTrackingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(
        ConsumeContext<OrderPlaced> context)
    {
        var message = context.Message;

        var totalQuantity =
            message.Items.Sum(
                item => item.Quantity);

        var shopperEvent =
            new ShopperEvent
            {
                CustomerId =
                    message.CustomerId,

                EventType =
                    "OrderPlaced",

                OrderId =
                    message.OrderId,

                Quantity =
                    totalQuantity,

                OccurredAt =
                    message.OccurredAt.UtcDateTime,

                Metadata =
                    JsonSerializer.Serialize(
                        new
                        {
                            message.OrderId,
                            message.TotalAmount,
                            Items = message.Items
                        })
            };

        await _dbContext.ShopperEvents.AddAsync(
            shopperEvent,
            context.CancellationToken);

        await _dbContext.SaveChangesAsync(
            context.CancellationToken);
    }
}