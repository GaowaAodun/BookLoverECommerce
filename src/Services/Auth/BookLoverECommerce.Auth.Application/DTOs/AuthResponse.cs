namespace BookLoverECommerce.Auth.Application.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } =
        new List<string>();
}