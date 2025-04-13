using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Infrastructure.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.JWT;
using BookingPlatform.Domain.Models;
using BookingPlatform.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace BookingPlatform.UnitTests.Services.Authentication
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IJwtTokenGenerator> _mockTokenGenerator;
        private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
        private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;
        private readonly IAuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockTokenGenerator = new Mock<IJwtTokenGenerator>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();
            _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
            
            _authService = new AuthenticationService(
                _mockUserRepo.Object,
                _mockTokenGenerator.Object,
                _mockLogger.Object,
                _mockPasswordHasher.Object);
        }

        #region Login Tests

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            const string email = "valid@test.com";
            const string password = "validPassword";
            var testUser = new User { Email = email };
            var expectedExpiration = DateTime.UtcNow.AddHours(1);

            _mockUserRepo.Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(testUser);
            
            _mockPasswordHasher.Setup(p => p.VerifyHashedPassword(testUser, It.IsAny<string>(), password))
                            .Returns(PasswordVerificationResult.Success);
            
            _mockTokenGenerator.Setup(g => g.GenerateToken(testUser))
                            .Returns(new JwtToken("valid_token", expectedExpiration, "booking-platform"));

            // Act
            var result = await _authService.LoginAsync(email, password);

            // Assert using FluentAssertions
            result.Token.Token.Should().Be("valid_token");
            result.Token.Expiration.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(1));
            result.Token.Issuer.Should().Be("booking-platform");
            result.Message.Should().Be("Login successful");
            
            _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => 
                    o.ToString()!.Contains($"User {email} logged in successfully")),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsInvalidPasswordException()
        {
            // Arrange
            const string email = "valid@test.com";
            var testUser = new User { Email = email };

            _mockUserRepo.Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(testUser);
            
            _mockPasswordHasher.Setup(p => p.VerifyHashedPassword(testUser, It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(PasswordVerificationResult.Failed);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _authService.LoginAsync(email, "wrongPassword"));
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("valid@test.com", null)]
        [InlineData("valid@test.com", "")]
        public async Task LoginAsync_MissingCredentials_ThrowsArgumentException(string email, string password)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _authService.LoginAsync(email, password));
        }

        [Fact]
        public async Task LoginAsync_NonExistentUser_ThrowsUserNotFoundException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(
                () => _authService.LoginAsync("nonexistent@test.com", "anyPassword"));
        }

        #endregion

        #region Registration Tests

        [Fact]
        public async Task RegisterAsync_ValidRequest_CreatesUserWithCorrectDetails()
        {
            // Arrange
            const string email = "new@user.com";
            User createdUser = null!;

            _mockUserRepo.Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((User?)null);
            
            _mockUserRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                       .Callback<User, CancellationToken>((u, _) => createdUser = u)
                       .ReturnsAsync(true);
            
            _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<User>(), "ValidPass123!"))
                             .Returns("hashed_password");

            // Act
            await _authService.RegisterAsync("John", "Doe", email, "ValidPass123!");

            // Assert
            Assert.NotNull(createdUser);
            Assert.Equal("John", createdUser.FirstName);
            Assert.Equal("Doe", createdUser.LastName);
            Assert.Equal(email, createdUser.Email);
            Assert.Equal(UserRole.NormalUser, createdUser.Role);
            Assert.Equal("hashed_password", createdUser.PasswordHash);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsDuplicateEmailException()
        {
            // Arrange
            const string email = "exists@test.com";
            _mockUserRepo.Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new User());

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateEmailException>(
                () => _authService.RegisterAsync("John", "Doe", email, "ValidPass123!"));
        }

        [Theory]
        [InlineData(null, "Doe", "test@test.com", "pass")]
        [InlineData("John", null, "test@test.com", "pass")]
        [InlineData("John", "Doe", null, "pass")]
        [InlineData("John", "Doe", "test@test.com", null)]
        public async Task RegisterAsync_MissingRequiredFields_ThrowsArgumentException(
            string firstName, string lastName, string email, string password)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _authService.RegisterAsync(firstName, lastName, email, password));
        }

        [Fact]
        public async Task RegisterAsync_CreateUserFails_ThrowsUserRegistrationException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UserRegistrationException>(
                () => _authService.RegisterAsync("John", "Doe", "test@test.com", "ValidPass123!"));
        }

        [Fact]
        public async Task RegisterAsync_Success_LogsInformation()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            
            _mockUserRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            // Act
            await _authService.RegisterAsync("John", "Doe", "test@test.com", "ValidPass123!");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => 
                        o.ToString()!.Contains("User test@test.com registered successfully")),
                    It.IsAny<Exception?>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region Password Hashing Tests

        [Fact]
        public async Task LoginAsync_UsesPasswordHasherCorrectly()
        {
            // Arrange
            const string email = "test@test.com";
            const string password = "correctPassword";
            var testUser = new User { Email = email };

            _mockUserRepo.Setup(r => r.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(testUser);
            
            _mockPasswordHasher.Setup(p => p.VerifyHashedPassword(testUser, It.IsAny<string>(), password))
                             .Returns(PasswordVerificationResult.Success);

            // Act
            await _authService.LoginAsync(email, password);

            // Assert
            _mockPasswordHasher.Verify(p => p.VerifyHashedPassword(
                testUser, 
                It.IsAny<string>(), 
                password), 
                Times.Once);
        }

        #endregion
    }
}