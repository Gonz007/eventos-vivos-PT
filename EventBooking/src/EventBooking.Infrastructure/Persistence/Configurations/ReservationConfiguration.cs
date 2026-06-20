using EventBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventBooking.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.BuyerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.BuyerEmail)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(r => r.Quantity)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.ReservationCode)
            .HasMaxLength(20);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasOne(r => r.Event)
            .WithMany(e => e.Reservations)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
