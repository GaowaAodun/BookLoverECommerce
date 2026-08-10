using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

public sealed class ResetPasswordModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public ResetPasswordModel(
        IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public ResetPasswordRequest Input { get; set; } =
        new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(
        string? email,
        string? token)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage(
                "/Account/ForgotPassword");
        }

        Input.Email = email;
        Input.Token = token;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _authApiClient.ResetPasswordAsync(
                Input,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Your password has been reset successfully. You can now sign in.";

            return RedirectToPage(
                "/Account/Login");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Password could not be reset. {ex.Message}";

            return Page();
        }
    }
}