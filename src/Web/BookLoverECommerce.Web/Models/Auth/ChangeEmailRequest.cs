using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Auth;

public sealed class ChangeEmailRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "New email")]
    public string NewEmail { get; set; } = string.Empty;
}