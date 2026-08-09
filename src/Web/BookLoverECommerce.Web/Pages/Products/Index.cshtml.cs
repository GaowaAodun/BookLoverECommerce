using BookLoverECommerce.Web.Models.Products;
using BookLoverECommerce.Web.Services.Interfaces;
//using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookLoverECommerce.Web.Pages.Products;


public sealed class IndexModel : PageModel
{
    private readonly IProductsApiClient _productsApiClient;

    public IndexModel(
        IProductsApiClient productsApiClient)
    {
        _productsApiClient = productsApiClient;
    }

    public IReadOnlyList<ProductViewModel> Products
        { get; private set; } = [];

    public string? Search { get; set; }

    public string? Sort { get; set; }
    public string? Category { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(
    string? search,
    string? category,
    string? sort,
    CancellationToken cancellationToken)
    {
        Search = search;
        Category = category;
        Sort = sort;

        try
        {
            var products =
                (await _productsApiClient
                    .GetProductsAsync(cancellationToken))
                .ToList();

            // Search by title or author
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products
                    .Where(product =>
                        product.Title.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        product.Author.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            if (!string.IsNullOrWhiteSpace(category))
{
    products = products
        .Where(product =>
            string.Equals(
                product.Category,
                category,
                StringComparison.OrdinalIgnoreCase))
        .ToList();
}

            // Sort products
            products = sort switch
            {
                "price_asc" =>
                    products
                        .OrderBy(p => p.Price)
                        .ToList(),

                "price_desc" =>
                    products
                        .OrderByDescending(p => p.Price)
                        .ToList(),

                "name_asc" =>
                    products
                        .OrderBy(p => p.Title)
                        .ToList(),

                _ => products
            };

            Products = products;
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "Products could not be loaded. Please try again.";
        }
    }
}