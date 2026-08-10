using BookLoverECommerce.Web.Models.Orders;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Orders;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly IOrderApiClient _OrderApiClient;

    public IndexModel(
        IOrderApiClient OrderApiClient)
    {
        _OrderApiClient = OrderApiClient;
    }

    public IReadOnlyList<OrderViewModel> Orders
        { get; private set; } =
        Array.Empty<OrderViewModel>();

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        Orders =
            await _OrderApiClient.GetOrdersAsync(
                cancellationToken);
    }
}