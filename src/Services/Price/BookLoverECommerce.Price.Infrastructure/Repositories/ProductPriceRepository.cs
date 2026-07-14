using BookLoverECommerce.Price.Application.Abstractions;
using BookLoverECommerce.Price.Domain.Entities;
using BookLoverECommerce.Price.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Price.Infrastructure.Repositories;

public sealed class ProductPriceRepository
    : IProductPriceRepository
{
    private readonly PriceDbContext _dbContext;

    public ProductPriceRepository(PriceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ProductPrice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductPrices
            .AsNoTracking()
            .OrderBy(productPrice => productPrice.ProductId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<ProductPrice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductPrices
            .FirstOrDefaultAsync(
                productPrice => productPrice.Id == id,
                cancellationToken);
    }

    public Task<ProductPrice?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductPrices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                productPrice => productPrice.ProductId == productId,
                cancellationToken);
    }

    public Task<bool> ExistsForProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductPrices.AnyAsync(
            productPrice => productPrice.ProductId == productId,
            cancellationToken);
    }

    public async Task AddAsync(
        ProductPrice productPrice,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductPrices.AddAsync(
            productPrice,
            cancellationToken);
    }

    public void Remove(ProductPrice productPrice)
    {
        _dbContext.ProductPrices.Remove(productPrice);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
