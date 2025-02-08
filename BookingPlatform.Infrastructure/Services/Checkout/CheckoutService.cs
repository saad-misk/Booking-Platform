using BookingPlatform.Application.DTOs.Checkout.Requests;
using BookingPlatform.Application.DTOs.Checkout.Responses;
using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Application.Interfaces.HelperServices.Payment;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Services.Checkout
{
    public class CheckoutService : ISingleItemCheckoutService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IRoomsRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            ICartRepository cartRepo,
            IRoomsRepository roomRepo,
            IBookingRepository bookingRepository,
            IPaymentService paymentService,
            IInvoiceService invoiceService,
            IEmailService emailService,
            IUserRepository userRepo,
            IUnitOfWork unitOfWork,
            ILogger<CheckoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _bookingRepository = bookingRepository;
            _cartRepository = cartRepo;
            _userRepository = userRepo;
            _roomRepository = roomRepo;
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<CheckoutResponse> ProcessSingleCartItemAsync(Guid cartId, Guid cartItemId, CheckoutRequest request)
        {
            var userId = (await _userRepository.GetByIdAsync(request.UserId))?.UserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException("User not authenticated");

            _logger.LogInformation(
                    "Processing checkout for user {UserId}, cart {CartId}, item {CartItemId}",
                    userId, cartId, cartItemId);
            // 1. Retrieve cart and specific item
            var cart = await _cartRepository.GetByIdAsync(cartId);
            var cartItem = cart?.Items.FirstOrDefault(i => i.CartItemId == cartItemId)
                ?? throw new NotFoundException($"Cart item {cartItemId} not found");

            // 2. Validate room availability
            var room = await _roomRepository.GetByIdAsync(cartItem.RoomId);
            if (room == null)
                throw new NotFoundException($"Room {cartItem.RoomId} not found");

            var isAvailable = await _roomRepository.IsRoomAvailableAsync(
                cartItem.RoomId,
                cartItem.CheckInDate,
                cartItem.CheckOutDate);

            if (!isAvailable)
                throw new ConflictException($"Room {cartItem.RoomId} is no longer available");

            // 3. Create booking
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                UserId = (Guid)userId!,
                HotelId = room.HotelId,
                RoomId = cartItem.RoomId,
                CheckInDateUtc = cartItem.CheckInDate.ToUniversalTime(),
                CheckOutDateUtc = cartItem.CheckOutDate.ToUniversalTime(),
                TotalPrice = cartItem.TotalPrice,
                CreatedAtUtc = DateTime.UtcNow,
                ConfirmationNumber = GenerateConfirmationNumber(),
                Status = BookingStatus.Pending, // Will be confirmed after payment
            };

            // 4. Process payment
            var payment = await ProcessPaymentAsync(booking, request.PaymentMethod);
            booking.Status = BookingStatus.Confirmed; // Update status after successful payment

            // 5. Generate invoice
            var invoice = await GenerateInvoiceAsync(booking);

            await _unitOfWork.BeginTransactionAsync();
            try
            {

                // 6. Save entities
                await _bookingRepository.AddAsync(booking);

                // 7. Remove processed item from cart
                await _cartRepository.RemoveCartItemAsync(cartId, cartItemId);
                await _unitOfWork.CommitAsync();

                // 8. Send confirmation email
                await _emailService.SendBookingConfirmationAsync(
                    request.Email,
                    booking,
                    invoice.FilePath);

                _logger.LogInformation("Successfully processed cart item {CartItemId}", cartItemId);

                return CreateSuccessResponse(booking, room, invoice, payment);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to process cart item {CartItemId}", cartItemId);
                await _paymentService.RefundPaymentAsync(payment.TransactionId);
                return CreateErrorResponse(ex, cartItemId);
            }
        }

        private async Task<Payment> ProcessPaymentAsync(Booking booking, PaymentMethod paymentMethod)
        {
            var paymentResult = await _paymentService.ProcessPaymentAsync(
                booking.TotalPrice,
                paymentMethod);

            if (paymentResult.Status != PaymentStatus.Confirmed)
                throw new PaymentException("Payment processing failed");

            return new Payment
            {
                PaymentId = Guid.NewGuid(),
                BookingId = booking.BookingId,
                Amount = booking.TotalPrice,
                PaymentMethod = paymentMethod,
                TransactionId = paymentResult.TransactionId,
                Status = paymentResult.Status,
                PaymentDate = DateTime.UtcNow
            };
        }

        private async Task<Invoice> GenerateInvoiceAsync(Booking booking)
        {
            var invoicePath = await _invoiceService.GenerateAndSaveInvoiceAsync(
                booking.UserId, booking.BookingId);

            return new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                BookingId = booking.BookingId,
                FilePath = invoicePath,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private CheckoutResponse CreateSuccessResponse(Booking booking, Room room, Invoice invoice, Payment payment)
        {
            return new CheckoutResponse
            {
                Success = true,
                ConfirmationNumber = booking.ConfirmationNumber,
                CheckInDate = booking.CheckInDateUtc,
                CheckOutDate = booking.CheckOutDateUtc,
                HotelName = room.Hotel.Name,
                RoomType = room.RoomClass,
                TotalPrice = booking.TotalPrice,
                PaymentStatus = payment.Status,
                InvoiceUrl = invoice.FilePath
            };
        }

        private CheckoutResponse CreateErrorResponse(Exception ex, Guid cartItemId)
        {
            return new CheckoutResponse
            {
                Success = false,
                ErrorMessage = ex is PaymentException ? ex.Message : "Checkout process failed",
                PaymentStatus = ex is PaymentException ? PaymentStatus.Failed : PaymentStatus.Pending
            };
        }

        private string GenerateConfirmationNumber() =>
            $"{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}