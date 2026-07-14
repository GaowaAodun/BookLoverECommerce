using BookLoverECommerce.Auth.Application.DTOs;

namespace BookLoverECommerce.Auth.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse?> LoginAsync(LoginRequest request);
}