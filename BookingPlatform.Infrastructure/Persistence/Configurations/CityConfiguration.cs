using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(c => c.CityId);
            
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(c => c.Thumbnail)
                .WithMany()
                .HasForeignKey("ThumbnailId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.Images)
                .WithOne()
                .HasForeignKey("CityId")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}