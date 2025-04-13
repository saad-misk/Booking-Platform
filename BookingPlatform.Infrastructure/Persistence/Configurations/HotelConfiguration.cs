using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.ToTable("Hotels");
            
            builder.HasMany(h => h.Rooms)
                .WithOne(r => r.Hotel)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasMany(h => h.Bookings)
                .WithOne(b => b.Hotel)
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(h => h.Gallery)
            .WithOne()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(h => h.Thumbnail)
            .WithMany()
            .HasForeignKey("ThumbnailId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        }
    }
}