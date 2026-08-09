using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cms.Models.Categories;

public sealed class CategoryFormModel
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Category name must contain between 2 and 100 characters.")]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Range(
        0,
        9999,
        ErrorMessage = "Display order must be between 0 and 9999.")]
    [Display(Name = "Display order")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Parent category")]
    public int? ParentCategoryId { get; set; }
}