using BookLoverECommerce.Web.Models.Auth;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Account;

public sealed class RegisterModel : PageModel
{
    private readonly IAuthApiClient _authApiClient;

    public RegisterModel(IAuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public RegisterRequest Input { get; set; } = new();

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
            await _authApiClient.RegisterAsync(
                Input,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Account created successfully. Please sign in.";

            return RedirectToPage("/Account/Login");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return Page();
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Registration service is unavailable.");

            return Page();
        }
    }
}