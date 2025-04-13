using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Interfaces.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

public class InvoiceService : IInvoiceService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly string _invoicesDirectory;

    public InvoiceService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
        _invoicesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "invoices");

        if (!Directory.Exists(_invoicesDirectory))
            Directory.CreateDirectory(_invoicesDirectory);
    }

    /// Generates an invoice as a PDF and returns it as a byte array (for API download).
    public async Task<byte[]> GenerateInvoiceAsync(Guid userId, Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking == null || booking.UserId != userId)
            throw new ArgumentException("Booking not found or access denied.");

        var pdfBytes = GenerateInvoicePdf(booking);
        return pdfBytes;
    }

    /// Generates an invoice as a PDF and saves it to disk (for email attachments).
    public async Task<string> GenerateAndSaveInvoiceAsync(Guid userId, Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);

        if (booking == null || booking.UserId != userId)
            throw new ArgumentException("Booking not found or access denied.");

        var pdfBytes = GenerateInvoicePdf(booking);
        var filePath = Path.Combine(_invoicesDirectory, $"Invoice_{bookingId}.pdf");

        await File.WriteAllBytesAsync(filePath, pdfBytes);
        return filePath;
    }

    /// Private method that creates the invoice PDF.
    private byte[] GenerateInvoicePdf(Booking booking)
    {
        var nights = (booking.CheckOutDateUtc - booking.CheckInDateUtc).Days;
        var total = nights * booking.Room.PricePerNight;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Content().Column(column =>
                {
                    column.Item().Text("Invoice").FontSize(20).Bold();
                    column.Item().Text($"User: {booking.UserId}").FontSize(14);
                    column.Item().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}");

                    column.Item().PaddingVertical(5).LineHorizontal(1);

                    column.Item().Text("Booking Details").FontSize(16).Bold();
                    column.Item().Text($"ID: {booking.BookingId}");
                    column.Item().Text($"Hotel: {booking.Room.Hotel.Name}");
                    column.Item().Text($"Room: {booking.Room.RoomClass}");
                    column.Item().Text($"Check-in: {booking.CheckInDateUtc:d}");
                    column.Item().Text($"Check-out: {booking.CheckOutDateUtc:d}");
                    column.Item().Text($"Price/Night: {booking.Room.PricePerNight:C}");
                    column.Item().Text($"Nights: {nights}");

                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    column.Item().Text($"Total: {total:C}").FontSize(14).Bold().AlignRight();
                });

                page.Footer().AlignCenter().Text("Thank you for booking with us!");
            });
        });

        return document.GeneratePdf();
    }
}