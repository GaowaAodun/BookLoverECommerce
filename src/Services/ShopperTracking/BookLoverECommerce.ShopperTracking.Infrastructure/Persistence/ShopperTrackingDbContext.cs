using BookLoverECommerce.ShopperTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.ShopperTracking.Infrastructure.Persistence;

public sealed class ShopperTrackingDbContext : DbContext
{
    public ShopperTrackingDbContext(
        DbContextOptions<ShopperTrackingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShopperEvent> ShopperEvents =>
        Set<ShopperEvent>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShopperEvent>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.EventType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Metadata)
                .HasColumnType("text");

            entity.HasIndex(x => x.CustomerId);

            entity.HasIndex(x => x.EventType);

            entity.HasIndex(x => x.OccurredAt);
        });
    }
}