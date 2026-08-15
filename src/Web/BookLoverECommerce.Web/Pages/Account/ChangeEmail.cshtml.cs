using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

[Authorize]
public sealed class ChangeEmailModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public ChangeEmailModel(
        IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public ChangeEmailRequest Input { get; set; } = new();

    public string? CurrentEmail { get; private set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var profile =
                await _authApiClient.GetProfileAsync(
                    cancellationToken);

            if (profile is null)
            {
                return NotFound();
            }

            CurrentEmail = profile.Email;

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Account information could not be loaded. {ex.Message}";

            return Page();
        }
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
            await _authApiClient.ChangeEmailAsync(
                Input,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Your email address has been updated successfully.";

            return RedirectToPage(
                "/Account/Profile");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Email could not be updated. {ex.Message}";

            return Page();
        }
    }
}