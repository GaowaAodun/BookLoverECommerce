using BookLoverECommerce.Web.Models.Products;
using BookLoverECommerce.Web.Services.Interfaces;
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

    public string? Search { get; private set; }

    public string? Sort { get; private set; }

    public string? Category { get; private set; }

    public string? ErrorMessage { get; private set; }

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
                (await _productsApiClient.GetProductsAsync(
                    cancellationToken))
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

            // Category filter
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

            // Sorting
            products = sort switch
            {
                "price_asc" =>
                    products
                        .OrderBy(product => product.Price)
                        .ToList(),

                "price_desc" =>
                    products
                        .OrderByDescending(product => product.Price)
                        .ToList(),

                "name_asc" =>
                    products
                        .OrderBy(product => product.Title)
                        .ToList(),

                _ => products
            };

            Products = products;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Products could not be loaded. {ex.Message}";

            Products = [];
        }
    }
}