using BookLoverECommerce.Order.Application.DTOs;
using BookLoverECommerce.Order.Application.Interfaces;
using BookLoverECommerce.Order.Domain.Entities;

namespace BookLoverECommerce.Order.Application.Services;

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IProductsClient _productsClient;
    private readonly IPriceClient _priceClient;

    public OrderService(
        IOrderRepository repository,
        IProductsClient productsClient,
        IPriceClient priceClient)
    {
        _repository = repository;
        _productsClient = productsClient;
        _priceClient = priceClient;
    }

    public async Task<OrderDto> CreateAsync(
        string customerId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Customer ID is required.",
                nameof(customerId));
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException(
                "An order must contain at least one item.",
                nameof(request));
        }

        if (request.Items.Any(item =>
                item.ProductId == Guid.Empty))
        {
            throw new ArgumentException(
                "Every order item must contain a valid product ID.",
                nameof(request));
        }

        if (request.Items.Any(item =>
                item.Quantity <= 0))
        {
            throw new ArgumentException(
                "Every order item quantity must be greater than zero.",
                nameof(request));
        }

        var duplicateProductId = request.Items
            .GroupBy(item => item.ProductId)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicateProductId.HasValue)
        {
            throw new ArgumentException(
                $"Product '{duplicateProductId}' appears more than once.",
                nameof(request));
        }

        var orderItems = new List<OrderItem>();

        foreach (var requestedItem in request.Items)
        {
            var product = await _productsClient.GetProductAsync(
                requestedItem.ProductId,
                cancellationToken);

            if (product is null)
            {
                throw new ArgumentException(
                    $"Product '{requestedItem.ProductId}' was not found.",
                    nameof(request));
            }

            if (product.StockQuantity <
                requestedItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Product '{product.Name}' does not have enough stock. " +
                    $"Available: {product.StockQuantity}, " +
                    $"requested: {requestedItem.Quantity}.");
            }

            var price = await _priceClient.GetPriceAsync(
                requestedItem.ProductId,
                cancellationToken);

            if (price is null)
            {
                throw new InvalidOperationException(
                    $"No price was found for product '{product.Name}'.");
            }

            if (price.ProductId != product.Id)
            {
                throw new InvalidOperationException(
                    $"The returned price does not belong to product " +
                    $"'{product.Name}'.");
            }

            if (price.EffectivePrice < 0)
            {
                throw new InvalidOperationException(
                    $"Product '{product.Name}' has an invalid price.");
            }

            orderItems.Add(
                new OrderItem(
                    product.Id,
                    product.Name,
                    price.EffectivePrice,
                    requestedItem.Quantity));
        }

        var addressRequest = request.ShippingAddress
     ?? throw new ArgumentException(
         "Shipping address is required.",
         nameof(request));

        var shippingAddress = new ShippingAddress(
            addressRequest.RecipientName,
            addressRequest.PhoneNumber,
            addressRequest.AddressLine1,
            addressRequest.AddressLine2,
            addressRequest.City,
            addressRequest.Province,
            addressRequest.PostalCode,
            addressRequest.Country);

        var order = new Domain.Entities.Order(
            customerId,
            orderItems,
            shippingAddress);

        await _repository.AddAsync(
            order,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Map(order);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetMineAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        return orders
            .Select(Map)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetAllAsync(
            cancellationToken);

        return orders
            .Select(Map)
            .ToArray();
    }

    public async Task<bool> MarkPaymentSucceededAsync(
    Guid orderId,
    string paymentReference,
    CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.MarkPaymentSucceeded(paymentReference);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartProcessingAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.StartProcessing();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ShipAsync(
        Guid orderId,
        ShipOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await _repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.Ship(
            request.Carrier,
            request.TrackingNumber,
            request.TrackingUrl);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ConfirmReceiptAsync(
        Guid orderId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.ConfirmReceipt(customerId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.Complete();

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelByCustomerAsync(
        Guid orderId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var order = await _repository.GetByIdAsync(
            orderId,
            cancellationToken);

        if (order is null)
        {
            return false;
        }

        order.CancelByCustomer(customerId);

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
    private static OrderDto Map(
        Domain.Entities.Order order)
    {
        var items = order.Items
            .Select(item => new OrderItemDto(
                item.Id,
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity,
                item.Subtotal))
            .ToArray();

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            items);
    }
}