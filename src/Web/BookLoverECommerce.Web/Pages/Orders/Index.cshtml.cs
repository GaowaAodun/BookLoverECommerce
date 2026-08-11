using BookLoverECommerce.Web.Models.Orders;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Orders;

public sealed class IndexModel : PageModel
{
    private readonly IOrderApiClient _orderApiClient;

    public IndexModel(
        IOrderApiClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }

    public IReadOnlyCollection<OrderViewModel> Orders
        { get; private set; }
        = Array.Empty<OrderViewModel>();

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Orders =
                await _orderApiClient.GetOrdersAsync(
                    cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            ErrorMessage =
                $"Orders could not be loaded. {exception.Message}";
        }
    }
}