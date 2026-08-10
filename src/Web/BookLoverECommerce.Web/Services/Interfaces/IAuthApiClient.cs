using BookLoverECommerce.Web.Models.Auth;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface IAuthApiClient
{
    Task<AuthResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
        Task<ProfileResponse?> GetProfileAsync(
    CancellationToken cancellationToken = default);

Task<ProfileResponse?> UpdateProfileAsync(
    UpdateProfileRequest request,
    CancellationToken cancellationToken = default);
    Task ChangeEmailAsync(
    ChangeEmailRequest request,
    CancellationToken cancellationToken = default);

Task ChangePasswordAsync(
    ChangePasswordRequest request,
    CancellationToken cancellationToken = default);
    Task<ForgotPasswordResponse> ForgotPasswordAsync(
    ForgotPasswordRequest request,
    CancellationToken cancellationToken = default);

Task ResetPasswordAsync(
    ResetPasswordRequest request,
    CancellationToken cancellationToken = default);
}