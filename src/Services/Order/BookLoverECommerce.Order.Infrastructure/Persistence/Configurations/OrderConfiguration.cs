using BookLoverECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLoverECommerce.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration
    : IEntityTypeConfiguration<Domain.Entities.Order>
{
    public void Configure(
        EntityTypeBuilder<Domain.Entities.Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(order => order.OrderNumber)
            .IsUnique();

        builder.Property(order => order.CustomerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(order => order.CustomerId);

        builder.Property(order => order.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(order => order.Status);

        builder.Property(order => order.PaymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(order => order.PaymentReference)
            .HasMaxLength(200);

        builder.Property(order => order.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(order => order.ShippingFee)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(order => order.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(order => order.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(order => order.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(order => order.AdminNote)
            .HasMaxLength(2000);

        builder.Property(order => order.PaidAt);
        builder.Property(order => order.ProcessingStartedAt);
        builder.Property(order => order.ShippedAt);
        builder.Property(order => order.DeliveredAt);
        builder.Property(order => order.CompletedAt);
        builder.Property(order => order.CancelledAt);

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        builder.Property(order => order.UpdatedAt)
            .IsRequired();

        builder.OwnsOne(
            order => order.ShippingAddress,
            address =>
            {
                address.Property(value => value.RecipientName)
                    .HasColumnName("ShippingRecipientName")
                    .HasMaxLength(150)
                    .IsRequired();

                address.Property(value => value.PhoneNumber)
                    .HasColumnName("ShippingPhoneNumber")
                    .HasMaxLength(30)
                    .IsRequired();

                address.Property(value => value.AddressLine1)
                    .HasColumnName("ShippingAddressLine1")
                    .HasMaxLength(250)
                    .IsRequired();

                address.Property(value => value.AddressLine2)
                    .HasColumnName("ShippingAddressLine2")
                    .HasMaxLength(250);

                address.Property(value => value.City)
                    .HasColumnName("ShippingCity")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(value => value.Province)
                    .HasColumnName("ShippingProvince")
                    .HasMaxLength(100)
                    .IsRequired();

                address.Property(value => value.PostalCode)
                    .HasColumnName("ShippingPostalCode")
                    .HasMaxLength(20)
                    .IsRequired();

                address.Property(value => value.Country)
                    .HasColumnName("ShippingCountry")
                    .HasMaxLength(100)
                    .IsRequired();
            });

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(order => order.Shipment)
            .WithOne()
            .HasForeignKey<Shipment>(shipment => shipment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Shipment)
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.HasMany(order => order.RefundRequests)
            .WithOne()
            .HasForeignKey(request => request.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.RefundRequests)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}