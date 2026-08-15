using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Auth.Application.DTOs;

public sealed class ChangeEmailRequest
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;
}