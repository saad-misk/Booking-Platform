using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Entities
{
    public class User{
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}