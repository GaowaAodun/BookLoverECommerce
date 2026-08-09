using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Application.DTOs;
using BookLoverECommerce.Products.Application.Exceptions;
using BookLoverECommerce.Products.Domain.Entities;
using BookLoverECommerce.Products.Domain.Enums;

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
        IReadOnlyCollection<Guid>? productIds,
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
                .Where(id => id != Guid.Empty)
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

    public async Task<IReadOnlyList<ProductDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(
            cancellationToken);

        return products
            .Select(MapToDto)
            .ToArray();
    }

    public async Task<ProductDto> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateProductType(command.ProductType);

        var categoryExists = await _categoryRepository.ExistsAsync(
            command.CategoryId,
            cancellationToken);

        if (!categoryExists)
        {
            throw new CategoryNotFoundException(command.CategoryId);
        }

        var normalizedSku = command.Sku
            .Trim()
            .ToUpperInvariant();

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

        await _productRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(
        Guid productId,
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateProductType(command.ProductType);

        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new ProductNotFoundException(productId);

        var categoryExists = await _categoryRepository.ExistsAsync(
            command.CategoryId,
            cancellationToken);

        if (!categoryExists)
        {
            throw new CategoryNotFoundException(command.CategoryId);
        }

        product.UpdateDetails(
            command.Name,
            command.Description,
            command.Price,
            command.CategoryId,
            command.ProductType,
            command.Brand,
            command.ThumbnailUrl);

        product.UpdateStock(command.StockQuantity);

        await _productRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(product);
    }

    public async Task DeleteAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrowAsync(
            productId,
            cancellationToken);

        _productRepository.Remove(product);

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ArchiveAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrowAsync(
            productId,
            cancellationToken);

        product.Archive();

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UnarchiveAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrowAsync(
            productId,
            cancellationToken);

        product.Unarchive();

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task PublishAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrowAsync(
            productId,
            cancellationToken);

        product.Publish();

        await _productRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Product> GetProductOrThrowAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new ProductNotFoundException(productId);
    }

    private static void ValidateProductType(
        ProductType productType)
    {
        if (!Enum.IsDefined(productType))
        {
            throw new ArgumentException(
                "A valid product type is required.",
                nameof(productType));
        }
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
}