using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cms.Models.Orders;

public sealed class OrderViewModel
{
    public Guid Id { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<OrderItemViewModel> Items { get; set; } = [];
}

public sealed class OrderItemViewModel
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }
}

public sealed class PaymentSucceededViewModel
{
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "Payment reference is required.")]
    [StringLength(200)]
    [Display(Name = "Payment Reference")]
    public string PaymentReference { get; set; } = string.Empty;
}

public sealed class ShipOrderViewModel
{
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "Carrier is required.")]
    [StringLength(100)]
    public string Carrier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tracking number is required.")]
    [StringLength(100)]
    [Display(Name = "Tracking Number")]
    public string TrackingNumber { get; set; } = string.Empty;

    [StringLength(500)]
    [Url(ErrorMessage = "Please enter a valid tracking URL.")]
    [Display(Name = "Tracking URL")]
    public string? TrackingUrl { get; set; }
}