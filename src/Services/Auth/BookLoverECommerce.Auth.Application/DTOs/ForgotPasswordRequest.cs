using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Auth.Application.DTOs;

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}