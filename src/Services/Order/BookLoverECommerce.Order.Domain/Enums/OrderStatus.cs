namespace BookLoverECommerce.Order.Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    Shipped = 2,
    Completed = 3,
    Cancelled = 4,
    Processing = 5,
    Delivered = 6
}