using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            // Primary Key
            builder.HasKey(ci => ci.CartItemId);

            // Foreign Key to Cart
            builder.HasOne(ci => ci.Cart)
                   .WithMany(c => c.Items)
                   .HasForeignKey(ci => ci.CartId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Foreign Key to Room
            builder.HasOne(ci => ci.Room)
                   .WithMany()
                   .HasForeignKey(ci => ci.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Required Fields
            builder.Property(ci => ci.CheckInDate)
                   .IsRequired();

            builder.Property(ci => ci.CheckOutDate)
                   .IsRequired();

            builder.Property(ci => ci.TotalPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(ci => ci.AddedAtUtc)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}