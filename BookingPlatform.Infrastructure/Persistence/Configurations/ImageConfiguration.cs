using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.HasKey(i => i.ImageId);

            // Constraints
            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Url)
                .IsRequired()
                .HasMaxLength(500); // For cloud storage URLs

            // Relationships
            // Hotel Relationship
            builder.HasOne(i => i.Hotel)
                .WithMany(h => h.Gallery)
                .HasForeignKey(i => i.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // City Relationship
            builder.HasOne(i => i.City)
                .WithMany(c => c.Images)
                .HasForeignKey(i => i.CityId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Room Relationship
            builder.HasOne(i => i.Room)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.RoomId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}