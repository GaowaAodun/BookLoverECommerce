using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

[Authorize]
public sealed class ChangePasswordModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public ChangePasswordModel(
        IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public ChangePasswordRequest Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
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
            await _authApiClient.ChangePasswordAsync(
                Input,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Your password has been updated successfully.";

            return RedirectToPage(
                "/Account/Profile");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Password could not be updated. {ex.Message}";

            return Page();
        }
    }
}