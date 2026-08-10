using System.Net;
using System.Net.Http.Json;
using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;

namespace BookLoverECommerce.Web.Services.ApiClients;

public sealed class AuthApiClient : IAuthApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AuthResponse?> RegisterAsync(
    RegisterRequest request,
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient("GatewayPublic");

    var apiRequest = new
    {
        request.FullName,
        request.UserName,
        request.Email,
        request.Password,
        request.ConfirmPassword
    };

    using var response =
        await client.PostAsJsonAsync(
            "/api/auth/register",
            apiRequest,
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (response.StatusCode == HttpStatusCode.BadRequest ||
        response.StatusCode == HttpStatusCode.Conflict)
    {
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(responseBody)
                ? "Registration could not be completed."
                : responseBody);
    }

    if (!response.IsSuccessStatusCode)
    {
        throw new InvalidOperationException(
            $"Registration failed. Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. Response: {responseBody}");
    }

    if (string.IsNullOrWhiteSpace(responseBody))
    {
        return null;
    }

    return System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(
        responseBody,
        new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
}

    public async Task<AuthResponse> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient("GatewayPublic");

    using var response =
        await client.PostAsJsonAsync(
            "/api/auth/login",
            request,
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(responseBody)
                ? "Invalid email or password."
                : responseBody);
    }

    if (response.StatusCode == HttpStatusCode.BadRequest)
    {
        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(responseBody)
                ? "The login request is invalid."
                : responseBody);
    }

    if (!response.IsSuccessStatusCode)
    {
        _logger.LogWarning(
            "Login failed. Status: {StatusCode}. Response: {Response}",
            response.StatusCode,
            responseBody);

        throw new InvalidOperationException(
            $"Login failed. Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. Response: {responseBody}");
    }

    var result =
        System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

    return result
        ?? throw new InvalidOperationException(
            "Authentication response was empty.");
}
public async Task<ProfileResponse?> GetProfileAsync(
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient(
            "GatewayAuthorized");

    using var response =
        await client.GetAsync(
            "/api/auth/profile",
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Get profile failed. " +
            $"Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. " +
            $"Response: {responseBody}");
    }

    return System.Text.Json.JsonSerializer
        .Deserialize<ProfileResponse>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
}
public async Task<ProfileResponse?> UpdateProfileAsync(
    UpdateProfileRequest request,
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient(
            "GatewayAuthorized");

    using var response =
        await client.PutAsJsonAsync(
            "/api/auth/profile",
            request,
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Update profile failed. " +
            $"Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. " +
            $"Response: {responseBody}");
    }

    return System.Text.Json.JsonSerializer
        .Deserialize<ProfileResponse>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
}
public async Task ChangeEmailAsync(
    ChangeEmailRequest request,
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient(
            "GatewayAuthorized");

    using var response =
        await client.PutAsJsonAsync(
            "/api/auth/profile/email",
            request,
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Email update failed. " +
            $"Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. " +
            $"Response: {responseBody}");
    }
}
public async Task ChangePasswordAsync(
    ChangePasswordRequest request,
    CancellationToken cancellationToken = default)
{
    var client =
        _httpClientFactory.CreateClient(
            "GatewayAuthorized");

    using var response =
        await client.PutAsJsonAsync(
            "/api/auth/profile/password",
            request,
            cancellationToken);

    var responseBody =
        await response.Content.ReadAsStringAsync(
            cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException(
            $"Password update failed. " +
            $"Status: {(int)response.StatusCode} " +
            $"{response.StatusCode}. " +
            $"Response: {responseBody}");
    }
}
}