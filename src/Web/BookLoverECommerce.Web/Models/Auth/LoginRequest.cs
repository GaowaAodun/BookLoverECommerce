using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Web.Models.Auth;

public sealed class LoginRequest
{
    [Required]
    [Display(Name = "Username or Email")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}