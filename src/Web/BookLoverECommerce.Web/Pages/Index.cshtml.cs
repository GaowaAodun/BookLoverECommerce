using BookLoverECommerce.Web.Models.Products;
using BookLoverECommerce.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IProductsApiClient _productsApiClient;

    public IndexModel(
        IProductsApiClient productsApiClient)
    {
        _productsApiClient = productsApiClient;
    }

    public IReadOnlyList<ProductViewModel> RecommendedProducts
        { get; private set; } = [];

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var products =
                await _productsApiClient.GetProductsAsync(
                    cancellationToken);

            RecommendedProducts =
                products
                    .Take(4)
                    .ToList();
        }
        catch
        {
            RecommendedProducts = [];
        }
    }
}