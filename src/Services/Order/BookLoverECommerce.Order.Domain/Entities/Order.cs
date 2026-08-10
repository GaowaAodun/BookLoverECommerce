using BookLoverECommerce.Order.Domain.Enums;

namespace BookLoverECommerce.Order.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    private readonly List<RefundRequest> _refundRequests = [];

    private Order() { }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string? PaymentReference { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; } = null!;
    public decimal Subtotal { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Shipment? Shipment { get; private set; }
    public string? AdminNote { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ProcessingStartedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<RefundRequest> RefundRequests => _refundRequests.AsReadOnly();

    public Order(
        string customerId,
        IEnumerable<OrderItem> items,
        ShippingAddress shippingAddress,
        decimal shippingFee = 0,
        decimal discountAmount = 0,
        decimal taxAmount = 0)
    {
        if (string.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        ArgumentNullException.ThrowIfNull(shippingAddress);

        var itemList = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        if (itemList.Count == 0) throw new ArgumentException("An order must contain at least one item.", nameof(items));
        if (shippingFee < 0 || discountAmount < 0 || taxAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(shippingFee), "Order amounts cannot be negative.");

        Id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        OrderNumber = $"ORD-{now:yyyyMMdd}-{Id.ToString("N")[..8].ToUpperInvariant()}";
        CustomerId = customerId.Trim();
        Status = OrderStatus.PendingPayment;
        PaymentStatus = PaymentStatus.Pending;
        ShippingAddress = shippingAddress;
        CreatedAt = now;
        UpdatedAt = now;
        _items.AddRange(itemList);
        Subtotal = _items.Sum(item => item.Subtotal);
        ShippingFee = shippingFee;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = Subtotal + ShippingFee + TaxAmount - DiscountAmount;
        if (TotalAmount < 0) throw new ArgumentException("Total amount cannot be negative.");
    }

    public void MarkPaymentSucceeded(string paymentReference)
    {
        if (Status != OrderStatus.PendingPayment || PaymentStatus != PaymentStatus.Pending)
            throw new InvalidOperationException("Only an order awaiting payment can be paid.");
        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException("Payment reference is required.", nameof(paymentReference));

        var now = DateTime.UtcNow;
        PaymentStatus = PaymentStatus.Paid;
        PaymentReference = paymentReference.Trim();
        Status = OrderStatus.Paid;
        PaidAt = now;
        UpdatedAt = now;
    }

    public void MarkPaymentFailed()
    {
        if (Status != OrderStatus.PendingPayment || PaymentStatus != PaymentStatus.Pending)
            throw new InvalidOperationException("Only a pending payment can fail.");
        PaymentStatus = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelByCustomer(string customerId)
    {
        EnsureOwnedBy(customerId);
        if (Status != OrderStatus.PendingPayment)
            throw new InvalidOperationException("A customer can only cancel an unpaid order.");

        Status = OrderStatus.Cancelled;
        CancelledAt = UpdatedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Only a paid order can start processing.");

        Status = OrderStatus.Processing;
        ProcessingStartedAt = UpdatedAt = DateTime.UtcNow;
    }

    public void Ship(string carrier, string trackingNumber, string? trackingUrl)
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only an order being processed can be shipped.");
        if (Shipment is not null)
            throw new InvalidOperationException("This order already has a shipment.");

        var now = DateTime.UtcNow;
        Shipment = new Shipment(Id, carrier, trackingNumber, trackingUrl, now);
        Status = OrderStatus.Shipped;
        ShippedAt = UpdatedAt = now;
    }

    public void ConfirmReceipt(string customerId)
    {
        EnsureOwnedBy(customerId);
        if (Status != OrderStatus.Shipped || Shipment is null)
            throw new InvalidOperationException("Only a shipped order can be confirmed as received.");

        var now = DateTime.UtcNow;
        Shipment.MarkDelivered(now);
        Status = OrderStatus.Delivered;
        DeliveredAt = UpdatedAt = now;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidOperationException("Only a delivered order can be completed.");

        Status = OrderStatus.Completed;
        CompletedAt = UpdatedAt = DateTime.UtcNow;
    }

    public RefundRequest RequestRefund(string customerId, string reason)
    {
        EnsureOwnedBy(customerId);
        if (PaymentStatus != PaymentStatus.Paid)
            throw new InvalidOperationException("Only a paid order can be refunded.");
        if (Status is OrderStatus.Cancelled or OrderStatus.PendingPayment)
            throw new InvalidOperationException("This order is not eligible for a refund.");
        if (_refundRequests.Any(request => request.Status is RefundStatus.Requested or RefundStatus.Approved))
            throw new InvalidOperationException("This order already has an active refund request.");

        var request = new RefundRequest(Id, CustomerId, TotalAmount, reason, DateTime.UtcNow);
        _refundRequests.Add(request);
        UpdatedAt = DateTime.UtcNow;
        return request;
    }

    public void ApproveRefund(Guid refundRequestId, string adminUserId, string? comment)
    {
        GetRefundRequest(refundRequestId).Approve(adminUserId, comment, DateTime.UtcNow);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RejectRefund(Guid refundRequestId, string adminUserId, string comment)
    {
        GetRefundRequest(refundRequestId).Reject(adminUserId, comment, DateTime.UtcNow);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteRefund(Guid refundRequestId, string adminUserId, string? comment)
    {
        var refund = GetRefundRequest(refundRequestId);
        refund.MarkRefunded(adminUserId, comment, DateTime.UtcNow);
        PaymentStatus = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAdminNote(string? note)
    {
        AdminNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnsureOwnedBy(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId) || !string.Equals(CustomerId, customerId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("The order does not belong to the current customer.");
    }

    private RefundRequest GetRefundRequest(Guid refundRequestId) =>
        _refundRequests.SingleOrDefault(request => request.Id == refundRequestId)
        ?? throw new KeyNotFoundException("Refund request was not found for this order.");
}