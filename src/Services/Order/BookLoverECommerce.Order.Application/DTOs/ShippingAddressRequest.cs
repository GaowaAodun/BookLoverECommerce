using System.ComponentModel.DataAnnotations;
public sealed class ShippingAddressRequest
{
    [Required]
    [StringLength(150)]
    public string RecipientName { get; init; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string AddressLine1 { get; init; } = string.Empty;

    [StringLength(250)]
    public string? AddressLine2 { get; init; }

    [Required]
    [StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Province { get; init; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PostalCode { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; init; } = string.Empty;
}