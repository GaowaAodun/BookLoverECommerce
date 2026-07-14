using BookLoverECommerce.Products.Application.Abstractions;
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
}