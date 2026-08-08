using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Cms.Pages;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    public string AdministratorName { get; private set; } =
        string.Empty;

    public void OnGet()
    {
        AdministratorName =
            User.FindFirst("FullName")?.Value
            ?? User.Identity?.Name
            ?? "Administrator";
    }
}