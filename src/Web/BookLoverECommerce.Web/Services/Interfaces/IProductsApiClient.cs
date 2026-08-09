using BookLoverECommerce.Web.Models.Products;

namespace BookLoverECommerce.Web.Services.Interfaces;

public interface IProductsApiClient
{
    Task<IReadOnlyList<ProductViewModel>> GetProductsAsync(
        CancellationToken cancellationToken = default);
}