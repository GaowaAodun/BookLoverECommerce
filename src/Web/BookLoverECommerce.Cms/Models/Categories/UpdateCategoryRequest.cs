using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cms.Models.Categories;

public sealed class UpdateCategoryRequest
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Parent category")]
    public int? ParentCategoryId { get; set; }
}