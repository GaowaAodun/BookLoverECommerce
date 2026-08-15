using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Auth;

public sealed class UpdateProfileRequest
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;
}