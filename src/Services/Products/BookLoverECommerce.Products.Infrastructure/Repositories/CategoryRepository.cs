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

    public Task<bool> NameExistsAsync(
        string name,
        int? excludingCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToUpper();

        return _dbContext.Categories.AnyAsync(
            category =>
                category.Name.ToUpper() == normalizedName &&
                (!excludingCategoryId.HasValue ||
                 category.Id != excludingCategoryId.Value),
            cancellationToken);
    }

    public Task<bool> HasProductsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products.AnyAsync(
            product => product.CategoryId == categoryId,
            cancellationToken);
    }

    public Task<bool> HasChildCategoriesAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.AnyAsync(
            category => category.ParentCategoryId == categoryId,
            cancellationToken);
    }

    public Task<Category?> GetByIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.SingleOrDefaultAsync(
            category => category.Id == categoryId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
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

    public Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories
            .AddAsync(category, cancellationToken)
            .AsTask();
    }

    public void Remove(Category category)
    {
        _dbContext.Categories.Remove(category);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}