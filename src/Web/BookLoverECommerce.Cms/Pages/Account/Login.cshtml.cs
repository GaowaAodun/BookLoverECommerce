using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BookLoverECommerce.Cms.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            Response.Redirect("/");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient("ApiGateway");

        HttpResponseMessage response;

        try
        {
            response = await client.PostAsJsonAsync(
                "/api/auth/login",
                Input);
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "The authentication service is unavailable.";
            return Page();
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Invalid username/email or password.";
            return Page();
        }

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage =
                "Login failed. Please try again.";
            return Page();
        }

        var authResponse =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        if (authResponse is null ||
            string.IsNullOrWhiteSpace(authResponse.Token))
        {
            ErrorMessage =
                "The authentication service returned an invalid response.";
            return Page();
        }

        if (!authResponse.Roles.Contains(
                "Admin",
                StringComparer.OrdinalIgnoreCase))
        {
            ErrorMessage =
                "You do not have permission to access the CMS.";
            return Page();
        }

        var claims = new List<Claim>
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

        claims.AddRange(
            authResponse.Roles.Select(
                role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var expiresUtc = new DateTimeOffset(
            DateTime.SpecifyKind(
                authResponse.ExpiresAt,
                DateTimeKind.Utc));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true
            });

        HttpContext.Session.SetString(
            "AccessToken",
            authResponse.Token);

        return LocalRedirect("/");
    }
}