using BookLoverECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLoverECommerce.Order.Infrastructure.Persistence.Configurations;

public sealed class RefundRequestConfiguration
    : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(
        EntityTypeBuilder<RefundRequest> builder)
    {
        builder.ToTable("RefundRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Id)
            .ValueGeneratedNever();

        builder.Property(request => request.OrderId)
            .IsRequired();

        builder.HasIndex(request => request.OrderId);

        builder.Property(request => request.CustomerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(request => request.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(request => request.Reason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(request => request.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(request => request.Status);

        builder.Property(request => request.AdminUserId)
            .HasMaxLength(450);

        builder.Property(request => request.AdminComment)
            .HasMaxLength(2000);

        builder.Property(request => request.RequestedAt)
            .IsRequired();

        builder.Property(request => request.ReviewedAt);
        builder.Property(request => request.RefundedAt);
    }
}