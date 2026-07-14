using BookLoverECommerce.Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Cart.Infrastructure.Persistence;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShoppingCart> ShoppingCarts =>
        Set<ShoppingCart>();

    public DbSet<CartItem> CartItems =>
        Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(cart => cart.Id);

            entity.Property(cart => cart.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.HasIndex(cart => cart.UserId)
                .IsUnique();

            entity.HasMany(cart => cart.Items)
                .WithOne(item => item.ShoppingCart)
                .HasForeignKey(item => item.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.Property(item => item.ProductId)
                .IsRequired();

            entity.Property(item => item.Quantity)
                .IsRequired();

            entity.HasIndex(item => new
            {
                item.ShoppingCartId,
                item.ProductId
            })
            .IsUnique();
        });
    }
}