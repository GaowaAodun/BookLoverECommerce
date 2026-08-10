using System.ComponentModel.DataAnnotations;

namespace BookLoverECommerce.Auth.Application.DTOs;

public sealed class UpdateProfileRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
}