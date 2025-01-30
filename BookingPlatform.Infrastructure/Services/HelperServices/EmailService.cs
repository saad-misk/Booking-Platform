using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BookingPlatform.Infrastructure.Services.HelperServices
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly SendGridClient _sendGridClient;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _sendGridClient = new SendGridClient(_emailSettings.ApiKey);
        }

        public async Task SendBookingConfirmationAsync(string email, Booking booking, string filePath)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Invoice file not found", filePath);

            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var to = new EmailAddress(email);
            var subject = "Booking Confirmation";
            var htmlContent = GetHtmlEmailTemplate(booking);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "Booking confirmed", htmlContent);

            // Attach invoice file
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileBase64 = Convert.ToBase64String(fileBytes);
            msg.AddAttachment(Path.GetFileName(filePath), fileBase64);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email. Status: {response.StatusCode}, Error: {errorMessage}");
            }
        }

        private string GetHtmlEmailTemplate(Booking booking)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2 style='color: #2E86C1;'>Booking Confirmation</h2>
                    <p>Dear {booking.User.FirstName},</p>
                    <p>Your booking has been successfully confirmed.</p>
                    <p><strong>Booking Details:</strong></p>
                    <ul>
                        <li><strong>Room ID:</strong> {booking.RoomId}</li>
                        <li><strong>Check-In Date:</strong> {booking.CheckInDateUtc:yyyy-MM-dd}</li>
                        <li><strong>Check-Out Date:</strong> {booking.CheckOutDateUtc:yyyy-MM-dd}</li>
                        <li><strong>Total Price:</strong> ${booking.TotalPrice:F2}</li>
                    </ul>
                    <p>Thank you for choosing our platform!</p>
                    <p>Best regards,</p>
                    <p><strong>Booking Platform Team</strong></p>
                </body>
                </html>";
        }
    }
}