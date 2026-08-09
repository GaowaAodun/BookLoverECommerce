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

    public LoginModel(IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result =
                await _authApiClient.LoginAsync(
                    Input,
                    cancellationToken);

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    result.UserId),

                new(
                    ClaimTypes.Name,
                    result.UserName),

                new(
                    ClaimTypes.Email,
                    result.Email),

                new(
                    ClaimTypes.Role,
                    result.Role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identity);

            var properties =
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,
                    ExpiresUtc =
                        result.ExpiresAt
                        ?? DateTimeOffset.UtcNow.AddMinutes(60)
                };

            properties.StoreTokens(
            [
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = result.Token
                }
            ]);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return Page();
        }
        catch (HttpRequestException ex)
{
    ModelState.AddModelError(
        string.Empty,
        $"Login service error: {ex.Message}");

    return Page();
}
    }
}