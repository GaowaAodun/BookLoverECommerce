using BookLoverECommerce.Price.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLoverECommerce.Price.Infrastructure.Persistence.Configurations;

public sealed class ProductPriceConfiguration
    : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(
        EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("product_prices");

        builder.HasKey(productPrice => productPrice.Id);

        builder.Property(productPrice => productPrice.Id)
            .HasColumnName("id");

        builder.Property(productPrice => productPrice.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.HasIndex(productPrice => productPrice.ProductId)
            .IsUnique();

        builder.Property(productPrice => productPrice.BasePrice)
            .HasColumnName("base_price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(productPrice => productPrice.SalePrice)
            .HasColumnName("sale_price")
            .HasPrecision(18, 2);

        builder.Property(productPrice => productPrice.Currency)
            .HasColumnName("currency")
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(productPrice => productPrice.SaleStartDate)
            .HasColumnName("sale_start_date");

        builder.Property(productPrice => productPrice.SaleEndDate)
            .HasColumnName("sale_end_date");

        builder.Property(productPrice => productPrice.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(productPrice => productPrice.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
