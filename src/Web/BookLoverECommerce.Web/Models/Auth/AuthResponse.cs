namespace BookLoverECommerce.Web.Models.Auth;

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}