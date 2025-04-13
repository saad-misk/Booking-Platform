using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingPlatform.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            // Primary Key
            builder.HasKey(u => u.UserId);

            builder.HasData(
                new User
                {
                    UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
                    UserName = "admin",
                    Role = UserRole.Admin,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@bookingplatform.com",
                    PasswordHash = "AQAAAAEAACcQAAAAEByh5Z5z7g7z8X6Q3wY1v2Ee.7z8X6Q3wY1v2Ee", //Admin123!
                    CreatedAtUtc = new DateTime(2023, 10, 1)
                },
                new User
                {
                    UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174001"),
                    UserName = "john_doe",
                    Role = UserRole.NormalUser,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PasswordHash = "AQAAAAEAACcQAAAAEByh5Z5z7g7z8X6Q3wY1v2Ee.7z8X6Q3wY1v2Ee", // John123!
                    CreatedAtUtc = new DateTime(2023, 10, 1)
                });

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.CreatedAtUtc)
                .IsRequired();

            builder.HasMany(u => u.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}