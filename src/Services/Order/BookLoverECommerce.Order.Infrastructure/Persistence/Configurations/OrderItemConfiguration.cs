using BookLoverECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLoverECommerce.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(
        EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired();

        builder.Property(item => item.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(item => item.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Ignore(item => item.Subtotal);

        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.ProductId);
    }
}