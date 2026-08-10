using BookLoverECommerce.Auth.Application.DTOs;

namespace BookLoverECommerce.Auth.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request);

    Task<AuthResponse?> LoginAsync(
        LoginRequest request);

    Task<ProfileResponse?> GetProfileAsync(
        string userId);

    Task<ProfileResponse?> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request);

    Task ChangeEmailAsync(
        string userId,
        ChangeEmailRequest request);

    Task ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request);
}