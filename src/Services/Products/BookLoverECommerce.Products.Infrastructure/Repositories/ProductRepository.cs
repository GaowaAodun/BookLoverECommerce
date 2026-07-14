using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Domain.Entities;
using BookLoverECommerce.Products.Domain.Enums;
using BookLoverECommerce.Products.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Products.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _dbContext;

    public ProductRepository(ProductsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Status == ProductStatus.Published ||
                product.Status == ProductStatus.OutOfStock)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken = default)
    {
        if (productIds.Count == 0)
        {
            return Array.Empty<Product>();
        }

        return await _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                productIds.Contains(product.Id) &&
                (
                product.Status == ProductStatus.Published ||
                product.Status == ProductStatus.OutOfStock
                ))
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products
            .FirstOrDefaultAsync(
                product => product.Id == id,
                cancellationToken);
    }

    public Task<bool> SkuExistsAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products.AnyAsync(
            product => product.Sku == sku,
            cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(
            product,
            cancellationToken);
    }

    public void Remove(Product product)
    {
        _dbContext.Products.Remove(product);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}