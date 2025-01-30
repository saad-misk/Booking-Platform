using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            // Primary Key
            builder.HasKey(h => h.HotelId);

            // Properties
            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(h => h.ReviewsRating)
                .HasColumnType("decimal(3,2)");

            builder.Property(h => h.Longitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(h => h.Latitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(h => h.Description)
                .HasMaxLength(1000);

            builder.Property(h => h.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(h => h.StarRating)
                .IsRequired();

            // Relationships
            builder.HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.City)
                .WithMany(c => c.Hotels)
                .HasForeignKey(h => h.CityId);

            builder.HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId);
        }
    }
}