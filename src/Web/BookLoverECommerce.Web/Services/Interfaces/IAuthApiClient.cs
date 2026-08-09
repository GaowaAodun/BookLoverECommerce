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
}