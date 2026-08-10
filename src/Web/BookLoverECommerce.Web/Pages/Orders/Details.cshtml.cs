using BookLoverECommerce.Web.Models.Orders;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Orders;

[Authorize]
public sealed class DetailsModel : PageModel
{
    private readonly IOrderApiClient _OrderApiClient;

    public DetailsModel(
        IOrderApiClient OrderApiClient)
    {
        _OrderApiClient = OrderApiClient;
    }

    public OrderViewModel? Order { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Order =
            await _OrderApiClient.GetOrderByIdAsync(
                id,
                cancellationToken);

        if (Order is null)
        {
            return NotFound();
        }

        return Page();
    }
}