using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoiceAsync(Guid userId, Guid bookingId);
        Task<string> GenerateAndSaveInvoiceAsync(Guid userId, Guid bookingId);
    }

}