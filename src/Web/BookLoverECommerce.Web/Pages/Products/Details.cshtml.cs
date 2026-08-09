using BookLoverECommerce.Web.Models.Products;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Products;

public sealed class DetailsModel : PageModel
{
    private readonly IProductsApiClient _productsApiClient;

    public DetailsModel(
        IProductsApiClient productsApiClient)
    {
        _productsApiClient = productsApiClient;
    }

    public ProductViewModel? Product { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var products =
                await _productsApiClient.GetProductsAsync(
                    cancellationToken);

            Product = products.FirstOrDefault(
                product => product.Id == id);

            if (Product is null)
            {
                return NotFound();
            }

            return Page();
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "The product could not be loaded.";

            return Page();
        }
    }
}