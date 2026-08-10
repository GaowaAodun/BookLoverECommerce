using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BookLoverECommerce.Cms.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client =
            _httpClientFactory.CreateClient(
                "ApiGateway");

        HttpResponseMessage response;

        try
        {
            response =
                await client.PostAsJsonAsync(
                    "/api/auth/login",
                    Input,
                    cancellationToken);
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "The authentication service is unavailable.";

            return Page();
        }

        if (response.StatusCode ==
            HttpStatusCode.Unauthorized)
        {
            ErrorMessage =
                "Invalid username/email or password.";

            return Page();
        }

        if (!response.IsSuccessStatusCode)
        {
            var body =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            ErrorMessage =
                $"Login failed. Status: " +
                $"{(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"{body}";

            return Page();
        }

        var authResponse =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>(
                    cancellationToken:
                        cancellationToken);

        if (authResponse is null ||
            string.IsNullOrWhiteSpace(
                authResponse.Token))
        {
            ErrorMessage =
                "The authentication service returned an invalid response.";

            return Page();
        }

        var isAdmin =
            authResponse.Roles.Any(
                role =>
                    string.Equals(
                        role,
                        "Admin",
                        StringComparison.OrdinalIgnoreCase));

        if (!isAdmin)
        {
            ErrorMessage =
                "You do not have permission to access the CMS.";

            return Page();
        }

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    authResponse.UserId),

                new(
                    ClaimTypes.Name,
                    authResponse.Username),

                new(
                    ClaimTypes.Email,
                    authResponse.Email),

                new(
                    "FullName",
                    authResponse.FullName)
            };

        foreach (var role in authResponse.Roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }
        }

        var identity =
            new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

        var principal =
            new ClaimsPrincipal(identity);

        var expiresUtc =
            new DateTimeOffset(
                DateTime.SpecifyKind(
                    authResponse.ExpiresAt,
                    DateTimeKind.Utc));

        var properties =
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true
            };

        properties.StoreTokens(
        [
            new AuthenticationToken
            {
                Name = "access_token",
                Value = authResponse.Token
            }
        ]);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            principal,
            properties);

        HttpContext.Session.SetString(
            "AccessToken",
            authResponse.Token);

        return RedirectToPage("/Index");
    }
}