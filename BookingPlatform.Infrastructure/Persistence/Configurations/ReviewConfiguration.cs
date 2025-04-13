using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);

            //RlationShips
            builder.HasOne(r => r.Hotel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HotelId);

            // Constraints
            builder.Property(r => r.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.Rating)
                .IsRequired()
                .HasAnnotation("Range", new[] { 1, 5 }); // Enforce rating range 1-5

            // Index for frequently filtered fields
            builder.HasIndex(r => r.HotelId);
            builder.HasIndex(r => r.CreatedAtUtc);
        }
    }
}