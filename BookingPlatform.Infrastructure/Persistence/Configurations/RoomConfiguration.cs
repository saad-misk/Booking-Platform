using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");
            builder.HasKey(r => r.RoomId);

            builder.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId);

            builder.Property(r => r.RoomClass)
                .HasConversion<string>(); // Store enum as string in DB

            builder.Property(r => r.Status)
                .HasConversion<string>();

            builder.Property(r => r.Number)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(r => r.PricePerNight)
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.Capacity)
                .IsRequired();
        
            builder.HasMany(r => r.Bookings)
                .WithOne(b => b.Room)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
            }
    }
}