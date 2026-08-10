using BookLoverECommerce.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookLoverECommerce.Order.Infrastructure.Persistence.Configurations;

public sealed class ShipmentConfiguration
    : IEntityTypeConfiguration<Shipment>
{
    public void Configure(
        EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.HasKey(shipment => shipment.Id);

        builder.Property(shipment => shipment.Id)
            .ValueGeneratedNever();

        builder.Property(shipment => shipment.OrderId)
            .IsRequired();

        builder.HasIndex(shipment => shipment.OrderId)
            .IsUnique();

        builder.Property(shipment => shipment.Carrier)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(shipment => shipment.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(shipment => shipment.TrackingNumber);

        builder.Property(shipment => shipment.TrackingUrl)
            .HasMaxLength(500);

        builder.Property(shipment => shipment.ShippedAt)
            .IsRequired();

        builder.Property(shipment => shipment.DeliveredAt);
    }
}