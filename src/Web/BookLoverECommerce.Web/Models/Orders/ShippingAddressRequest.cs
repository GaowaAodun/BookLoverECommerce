using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Orders;

public sealed class ShippingAddressRequest
{
    [Required]
    [StringLength(150)]
    [Display(Name = "Recipient name")]
    public string RecipientName { get; set; } =
        string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } =
        string.Empty;

    [Required]
    [StringLength(250)]
    [Display(Name = "Address line 1")]
    public string AddressLine1 { get; set; } =
        string.Empty;

    [StringLength(250)]
    [Display(Name = "Address line 2")]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; } =
        string.Empty;

    [Required]
    [StringLength(100)]
    public string Province { get; set; } =
        string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "Postal code")]
    public string PostalCode { get; set; } =
        string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } =
        "Canada";
}