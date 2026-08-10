using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

public sealed class ForgotPasswordModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public ForgotPasswordModel(
        IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public ForgotPasswordRequest Input { get; set; } =
        new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result =
                await _authApiClient
                    .ForgotPasswordAsync(
                        Input,
                        cancellationToken);

            if (!string.IsNullOrWhiteSpace(
                result.ResetToken))
            {
                return RedirectToPage(
                    "/Account/ResetPassword",
                    new
                    {
                        email = Input.Email,
                        token = result.ResetToken
                    });
            }

            TempData["SuccessMessage"] =
                result.Message;

            return RedirectToPage(
                "/Account/Login");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Password reset request failed. {ex.Message}";

            return Page();
        }
    }
}