using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Domain.Entities;
using BookLoverECommerce.Products.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Products.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly ProductsDbContext _dbContext;

    public CategoryRepository(ProductsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.AnyAsync(
            category =>
                category.Id == categoryId &&
                category.IsActive,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }
}