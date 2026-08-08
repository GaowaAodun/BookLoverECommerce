using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Cms.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Username or email is required.")]
    [Display(Name = "Username or email")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}