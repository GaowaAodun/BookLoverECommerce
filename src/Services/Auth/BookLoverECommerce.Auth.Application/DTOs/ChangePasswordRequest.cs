using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Auth.Application.DTOs;

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } =
        string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } =
        string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } =
        string.Empty;
}