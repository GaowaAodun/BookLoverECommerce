using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

[Authorize]
public sealed class EditProfileModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public EditProfileModel(
        IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public UpdateProfileRequest Input { get; set; } =
        new();

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

            Input = new UpdateProfileRequest
            {
                FullName = profile.FullName,
                Username = profile.Username
            };

            return Page();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Profile could not be loaded. {ex.Message}";

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
            var updated =
                await _authApiClient.UpdateProfileAsync(
                    Input,
                    cancellationToken);

            if (updated is null)
            {
                ErrorMessage =
                    "Profile could not be updated.";

                return Page();
            }

            TempData["SuccessMessage"] =
                "Your profile has been updated successfully.";

            return RedirectToPage(
                "/Account/Profile");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Profile update failed. {ex.Message}";

            return Page();
        }
    }
}