using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cms.Models.Products;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Range(
        typeof(decimal),
        "0",
        "79228162514264337593543950335")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock quantity")]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [EnumDataType(typeof(ProductType))]
    [Display(Name = "Product type")]
    public ProductType ProductType { get; set; } =
        ProductType.PrintedBook;

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "Enter a valid thumbnail URL.")]
    [Display(Name = "Thumbnail URL")]
    public string? ThumbnailUrl { get; set; }
}