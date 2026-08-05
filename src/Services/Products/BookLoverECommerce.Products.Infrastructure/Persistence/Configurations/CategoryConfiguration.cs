using BookLoverECommerce.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLoverECommerce.Products.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedOnAdd();

        builder.Property(category => category.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.DisplayOrder)
            .IsRequired();

        builder.Property(category => category.IsActive)
            .IsRequired();

        builder.Property(category => category.CreatedAtUtc)
            .IsRequired();

        builder.Property(category => category.UpdatedAtUtc);

        builder.HasIndex(category => category.Name);

        builder.HasOne(category => category.ParentCategory)
            .WithMany(category => category.ChildCategories)
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}