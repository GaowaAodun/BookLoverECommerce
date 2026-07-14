using BookLoverECommerce.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Products.Infrastructure.Persistence;

public sealed class ProductsDbContext : DbContext
{
    public ProductsDbContext(
        DbContextOptions<ProductsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProductsDbContext).Assembly);
    }
}