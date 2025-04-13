using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.HelperServices
{
    public interface IEmailService
    {
        Task SendBookingConfirmationAsync(
                    string Email,
                    Booking booking,
                    string FilePath);
    }
}