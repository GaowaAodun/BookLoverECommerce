using System.ComponentModel.DataAnnotations;
using BookLoverECommerce.Products.Domain.Enums;

namespace BookLoverECommerce.Products.Api.Contracts.Products;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Sku { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    public ProductType ProductType { get; init; }

    [StringLength(100)]
    public string? Brand { get; init; }

    [StringLength(500)]
    public string? ThumbnailUrl { get; init; }
}