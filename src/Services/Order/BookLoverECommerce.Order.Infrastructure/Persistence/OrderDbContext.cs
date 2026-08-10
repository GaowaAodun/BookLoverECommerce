using BookLoverECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Order.Infrastructure.Persistence;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain.Entities.Order> Orders =>
        Set<Domain.Entities.Order>();

    public DbSet<OrderItem> OrderItems =>
        Set<OrderItem>();

    public DbSet<Shipment> Shipments =>
        Set<Shipment>();

    public DbSet<RefundRequest> RefundRequests =>
        Set<RefundRequest>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderDbContext).Assembly);
    }
}