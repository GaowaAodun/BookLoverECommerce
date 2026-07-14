using BookLoverECommerce.Price.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Price.Infrastructure.Persistence;

public sealed class PriceDbContext : DbContext
{
    public PriceDbContext(
        DbContextOptions<PriceDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductPrice> ProductPrices =>
        Set<ProductPrice>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PriceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
