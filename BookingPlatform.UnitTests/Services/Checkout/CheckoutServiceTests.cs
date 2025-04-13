using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Application.DTOs.Checkout.Requests;
using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Application.Interfaces.HelperServices.Payment;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Services.Checkout;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookingPlatform.UnitTests.Services.Checkout
{
    public class CheckoutServiceTests
    {
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<IRoomsRepository> _roomRepositoryMock;
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<CheckoutService>> _loggerMock;
        private readonly CheckoutService _checkoutService;

        public CheckoutServiceTests()
        {
            _cartRepositoryMock = new Mock<ICartRepository>();
            _roomRepositoryMock = new Mock<IRoomsRepository>();
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _invoiceServiceMock = new Mock<IInvoiceService>();
            _emailServiceMock = new Mock<IEmailService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<CheckoutService>>();

            _checkoutService = new CheckoutService(
                _cartRepositoryMock.Object,
                _roomRepositoryMock.Object,
                _bookingRepositoryMock.Object,
                _paymentServiceMock.Object,
                _invoiceServiceMock.Object,
                _emailServiceMock.Object,
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_UserNotAuthenticated_ThrowsUnauthorizedException()
        {
            // Arrange: simulate an unauthenticated user by returning a User with an empty Guid.
            var requestUserId = Guid.NewGuid();
            _ = _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = Guid.Empty });

            var checkoutRequest = new CheckoutRequest
            {
                UserId = requestUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "test@example.com"
            };

            var cartId = Guid.NewGuid();
            var cartItemId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _checkoutService.ProcessSingleCartItemAsync(cartId, cartItemId, checkoutRequest));
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_CartItemNotFound_ThrowsNotFoundException()
        {
            // Arrange: valid user, but the cart does not contain the requested item.
            var validUserId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = validUserId });

            _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Cart { CartId = Guid.NewGuid(), Items = new List<CartItem>() });

            var checkoutRequest = new CheckoutRequest
            {
                UserId = validUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "test@example.com"
            };

            var cartId = Guid.NewGuid();
            var cartItemId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _checkoutService.ProcessSingleCartItemAsync(cartId, cartItemId, checkoutRequest));
            Assert.Contains(cartItemId.ToString(), exception.Message);
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_RoomNotFound_ThrowsNotFoundException()
        {
            // Arrange: cart item exists but the room cannot be found.
            var validUserId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = validUserId });

            var cartItemId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var cart = new Cart
            {
                CartId = Guid.NewGuid(),
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = cartItemId,
                        RoomId = roomId,
                        CheckInDate = DateTime.Today.AddDays(1),
                        CheckOutDate = DateTime.Today.AddDays(3),
                        TotalPrice = 100
                    }
                }
            };

            _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            // Return null to simulate room not found.
            _roomRepositoryMock.Setup(x => x.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Room?)null);

            var checkoutRequest = new CheckoutRequest
            {
                UserId = validUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "test@example.com"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _checkoutService.ProcessSingleCartItemAsync(cart.CartId, cartItemId, checkoutRequest));
            Assert.Contains(roomId.ToString(), exception.Message);
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_RoomNotAvailable_ThrowsConflictException()
        {
            // Arrange: cart and room exist but the room is unavailable.
            var validUserId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = validUserId });

            var cartItemId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var cart = new Cart
            {
                CartId = Guid.NewGuid(),
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = cartItemId,
                        RoomId = roomId,
                        CheckInDate = DateTime.Today.AddDays(1),
                        CheckOutDate = DateTime.Today.AddDays(3),
                        TotalPrice = 150
                    }
                }
            };

            _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var room = new Room
            {
                RoomId = roomId,
                HotelId = Guid.NewGuid(),
                RoomClass = RoomType.Budget
            };

            _roomRepositoryMock.Setup(x => x.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);

            _roomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(roomId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var checkoutRequest = new CheckoutRequest
            {
                UserId = validUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "test@example.com"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(
                () => _checkoutService.ProcessSingleCartItemAsync(cart.CartId, cartItemId, checkoutRequest));
            Assert.Contains(roomId.ToString(), exception.Message);
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_PaymentProcessingFails_ThrowsPaymentException()
        {
            // Arrange: cart, room, and availability are valid, but payment processing fails.
            var validUserId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = validUserId });

            var cartItemId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var cart = new Cart
            {
                CartId = Guid.NewGuid(),
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = cartItemId,
                        RoomId = roomId,
                        CheckInDate = DateTime.Today.AddDays(1),
                        CheckOutDate = DateTime.Today.AddDays(3),
                        TotalPrice = 200
                    }
                }
            };

            _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var room = new Room
            {
                RoomId = roomId,
                HotelId = Guid.NewGuid(),
                RoomClass = RoomType.Luxury
            };

            _roomRepositoryMock.Setup(x => x.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);
            _roomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(roomId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Simulate payment failure by returning a PaymentResult with a non-confirmed status.
            var failedPaymentResult = new PaymentResult
            {
                Status = PaymentStatus.Failed,
                TransactionId = "tx-failed"
            };

            _paymentServiceMock.Setup(x => x.ProcessPaymentAsync(cart.Items.First().TotalPrice, It.IsAny<PaymentMethod>()))
                .ReturnsAsync(failedPaymentResult);

            var checkoutRequest = new CheckoutRequest
            {
                UserId = validUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "test@example.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<PaymentException>(
                () => _checkoutService.ProcessSingleCartItemAsync(cart.CartId, cartItemId, checkoutRequest));
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_UnitOfWorkCommitFails_RollsBackAndRefundsPayment_ReturnsErrorResponse()
        {
            // Arrange: all steps succeed until the UnitOfWork.CommitAsync throws an exception.
            var validUserId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = validUserId });

            var cartItemId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var cart = new Cart
            {
                CartId = Guid.NewGuid(),
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = cartItemId,
                        RoomId = roomId,
                        CheckInDate = DateTime.Today.AddDays(2),
                        CheckOutDate = DateTime.Today.AddDays(4),
                        TotalPrice = 250
                    }
                }
            };

            _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var hotel = new Hotel { Name = "Test Hotel" };
            var room = new Room
            {
                RoomId = roomId,
                HotelId = Guid.NewGuid(),
                RoomClass = RoomType.Boutique,
                Hotel = hotel
            };

            _roomRepositoryMock.Setup(x => x.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);
            _roomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(roomId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Simulate successful payment processing.
            var successfulPaymentResult = new PaymentResult
            {
                Status = PaymentStatus.Confirmed,
                TransactionId = "tx-success"
            };
            _paymentServiceMock.Setup(x => x.ProcessPaymentAsync(cart.Items.First().TotalPrice, It.IsAny<PaymentMethod>()))
                .ReturnsAsync(successfulPaymentResult);

            // Simulate invoice generation.
            var invoicePath = "/invoices/invoice.pdf";
            _invoiceServiceMock.Setup(x => x.GenerateAndSaveInvoiceAsync(validUserId, It.IsAny<Guid>()))
                .ReturnsAsync(invoicePath);

            // Set up the UnitOfWork so that CommitAsync fails.
            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitAsync()).ThrowsAsync(new Exception("Commit failed"));
            _unitOfWorkMock.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

            var checkoutRequest = new CheckoutRequest
            {
                UserId = validUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "test@example.com"
            };

            // Act
            var response = await _checkoutService.ProcessSingleCartItemAsync(cart.CartId, cartItemId, checkoutRequest);

            // Assert: the response should indicate failure and the rollback and refund methods should be called.
            Assert.False(response.Success);
            Assert.Equal("Checkout process failed", response.ErrorMessage);
            Assert.Equal(PaymentStatus.Pending, response.PaymentStatus);
            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Once);
            _paymentServiceMock.Verify(x => x.RefundPaymentAsync("tx-success"), Times.Once);
        }

        [Fact]
        public async Task ProcessSingleCartItemAsync_SuccessfulProcessing_ReturnsSuccessResponse()
        {
            // Arrange: simulate a complete and successful checkout.
            var validUserId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { UserId = validUserId });

            var cartItemId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var cart = new Cart
            {
                CartId = Guid.NewGuid(),
                Items = new List<CartItem>
                {
                    new CartItem
                    {
                        CartItemId = cartItemId,
                        RoomId = roomId,
                        CheckInDate = DateTime.Today.AddDays(5),
                        CheckOutDate = DateTime.Today.AddDays(7),
                        TotalPrice = 300
                    }
                }
            };

            _cartRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var hotel = new Hotel { Name = "Luxury Hotel" };
            var room = new Room
            {
                RoomId = roomId,
                HotelId = Guid.NewGuid(),
                RoomClass = RoomType.Boutique,
                Hotel = hotel
            };

            _roomRepositoryMock.Setup(x => x.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(room);
            _roomRepositoryMock.Setup(x => x.IsRoomAvailableAsync(roomId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var successfulPaymentResult = new PaymentResult
            {
                Status = PaymentStatus.Confirmed,
                TransactionId = "tx-successful"
            };
            _paymentServiceMock.Setup(x => x.ProcessPaymentAsync(cart.Items.First().TotalPrice, It.IsAny<PaymentMethod>()))
                .ReturnsAsync(successfulPaymentResult);

            var invoicePath = "/invoices/invoice_success.pdf";
            _invoiceServiceMock.Setup(x => x.GenerateAndSaveInvoiceAsync(validUserId, It.IsAny<Guid>()))
                .ReturnsAsync(invoicePath);

            _unitOfWorkMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

            _bookingRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _cartRepositoryMock.Setup(x => x.RemoveCartItemAsync(cart.CartId, cartItemId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(x => x.SendBookingConfirmationAsync(It.IsAny<string>(), It.IsAny<Booking>(), invoicePath))
                .Returns(Task.CompletedTask);

            var checkoutRequest = new CheckoutRequest
            {
                UserId = validUserId,
                PaymentMethod = PaymentMethod.Card,
                Email = "customer@example.com"
            };

            // Act
            var response = await _checkoutService.ProcessSingleCartItemAsync(cart.CartId, cartItemId, checkoutRequest);

            // Assert
            Assert.True(response.Success);
            Assert.False(string.IsNullOrWhiteSpace(response.ConfirmationNumber));
            Assert.Equal(cart.Items.First().CheckInDate.ToUniversalTime(), response.CheckInDate);
            Assert.Equal(cart.Items.First().CheckOutDate.ToUniversalTime(), response.CheckOutDate);
            Assert.Equal(hotel.Name, response.HotelName);
            Assert.Equal(room.RoomClass, response.RoomType);
            Assert.Equal(cart.Items.First().TotalPrice, response.TotalPrice);
            Assert.Equal(PaymentStatus.Confirmed, response.PaymentStatus);
            Assert.Equal(invoicePath, response.InvoiceUrl);

            // Verify that key methods were called.
            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
            _bookingRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
            _cartRepositoryMock.Verify(x => x.RemoveCartItemAsync(cart.CartId, cartItemId, It.IsAny<CancellationToken>()), Times.Once);
            _emailServiceMock.Verify(x =>
                x.SendBookingConfirmationAsync(checkoutRequest.Email, It.IsAny<Booking>(), invoicePath),
                Times.Once);
        }
    }
}