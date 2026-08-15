using BookLoverECommerce.Web.Models.Orders;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Orders;

[Authorize]
public sealed class DetailsModel : PageModel
{
    private readonly IOrderApiClient _orderApiClient;

    public DetailsModel(
        IOrderApiClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }

    public OrderViewModel? Order { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            Order =
                await _orderApiClient.GetOrderByIdAsync(
                    id,
                    cancellationToken);

            if (Order is null)
            {
                ErrorMessage =
                    "The order could not be found.";
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"The Order service could not be reached: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"The order could not be loaded: {ex.Message}";
        }

        return Page();
    }
}