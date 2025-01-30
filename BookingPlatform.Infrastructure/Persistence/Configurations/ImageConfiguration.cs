using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.ToTable("Images");
            
            builder.HasKey(i => i.ImageId);
            
            builder.HasOne(i => i.Hotel)
                .WithMany(h => h.Gallery)
                .HasForeignKey(i => i.HotelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(i => i.City)
                .WithMany(c => c.Images)
                .HasForeignKey(i => i.CityId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(i => i.Room)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.RoomId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}