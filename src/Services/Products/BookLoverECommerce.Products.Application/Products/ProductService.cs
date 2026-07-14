using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Application.DTOs;
using BookLoverECommerce.Products.Application.Exceptions;
using BookLoverECommerce.Products.Domain.Entities;

namespace BookLoverECommerce.Products.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        IReadOnlyCollection<int>? productIds,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> products;

        if (productIds is null || productIds.Count == 0)
        {
            products = await _productRepository.GetPublishedAsync(
                cancellationToken);
        }
        else
        {
            var distinctIds = productIds
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            products = await _productRepository.GetByIdsAsync(
                distinctIds,
                cancellationToken);
        }

        return products
            .Select(MapToDto)
            .ToArray();
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(
            command.CategoryId,
            cancellationToken);

        if (!categoryExists)
        {
            throw new CategoryNotFoundException(command.CategoryId);
        }

        var normalizedSku = command.Sku.Trim().ToUpperInvariant();

        var skuExists = await _productRepository.SkuExistsAsync(
            normalizedSku,
            cancellationToken);

        if (skuExists)
        {
            throw new DuplicateSkuException(normalizedSku);
        }

        var product = new Product(
            command.Name,
            command.Description,
            normalizedSku,
            command.Price,
            command.StockQuantity,
            command.CategoryId,
            command.ProductType,
            command.CreatedByUserId,
            command.Brand,
            command.ThumbnailUrl);

        await _productRepository.AddAsync(
            product,
            cancellationToken);

        await _productRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    public async Task DeleteAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(productId);
        }

        _productRepository.Remove(product);

        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(productId);
        }

        product.Archive();

        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.Brand,
            product.Price,
            product.StockQuantity,
            product.CategoryId,
            product.ProductType,
            product.Status,
            product.ThumbnailUrl,
            product.CreatedByUserId,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }

    public async Task PublishAsync(
    int productId,
    CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(productId);
        }

        product.Publish();

        await _productRepository.SaveChangesAsync(cancellationToken);
    }

}