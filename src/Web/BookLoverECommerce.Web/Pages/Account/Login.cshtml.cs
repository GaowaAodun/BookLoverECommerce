using System.Security.Claims;
using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public LoginModel(
        IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet(
        string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        try
        {
            var result =
                await _authApiClient.LoginAsync(
                    Input,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(result.UserId))
            {
                throw new InvalidOperationException(
                    "Authentication response did not contain a user ID.");
            }

            if (string.IsNullOrWhiteSpace(result.Username))
            {
                throw new InvalidOperationException(
                    "Authentication response did not contain a username.");
            }

            if (string.IsNullOrWhiteSpace(result.Token))
            {
                throw new InvalidOperationException(
                    "Authentication response did not contain an access token.");
            }

            if (result.Roles is null ||
                result.Roles.Count == 0)
            {
                throw new InvalidOperationException(
                    "Authentication response did not contain any roles.");
            }

            // Create the normal user claims
            var claims =
                new List<Claim>
                {
                    new(
                        ClaimTypes.NameIdentifier,
                        result.UserId),

                    new(
                        ClaimTypes.Name,
                        result.Username),

                    new(
                        ClaimTypes.Email,
                        result.Email)
                };

            // Add EVERY role returned by Auth
            foreach (var role in result.Roles)
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

            var properties =
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,

                    ExpiresUtc =
                        result.ExpiresAt == default
                            ? DateTimeOffset.UtcNow.AddMinutes(60)
                            : new DateTimeOffset(result.ExpiresAt)
                };

            // Store JWT so GatewayAuthorized
            // can send it to Cart/Product APIs
            properties.StoreTokens(
            [
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = result.Token
                }
            ]);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                properties);

            var destination =
                returnUrl ?? ReturnUrl;

            if (!string.IsNullOrWhiteSpace(destination) &&
                Url.IsLocalUrl(destination))
            {
                return LocalRedirect(destination);
            }

            return RedirectToPage("/Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            ReturnUrl = returnUrl;

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Login service error: {ex.Message}");

            ReturnUrl = returnUrl;

            return Page();
        }
    }
}