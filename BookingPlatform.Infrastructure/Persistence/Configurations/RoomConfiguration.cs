using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.RoomId);

            // Relationships
            builder.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId);

            builder.Property(r => r.RoomClass)
                .HasConversion<string>(); // Store enum as string in DB

            builder.Property(r => r.Status)
                .HasConversion<string>();

            // Constraints
            builder.Property(r => r.Number)
                .IsRequired()
                .HasMaxLength(20); // e.g., "Room 101A"

            builder.Property(r => r.PricePerNight)
                .HasColumnType("decimal(18,2)"); // Precise currency storage

            builder.Property(r => r.Capacity)
                .IsRequired();

            // Indexes
            builder.HasIndex(r => new { r.HotelId, r.Number }).IsUnique(); // Unique room number per hotel
        }
    }
}