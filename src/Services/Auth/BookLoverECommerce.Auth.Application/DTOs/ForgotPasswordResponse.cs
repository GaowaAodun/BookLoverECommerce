namespace BookLoverECommerce.Auth.Application.DTOs;

public sealed class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;

    public string? ResetToken { get; set; }
}