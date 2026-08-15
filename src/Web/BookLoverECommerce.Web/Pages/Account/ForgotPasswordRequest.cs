using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Auth;

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;
}