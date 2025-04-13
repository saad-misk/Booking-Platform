using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Infrastructure.JWT;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookingPlatform.UnitTests.Services.Helpers
{
    public class JwtTokenGeneratorMoqTests
    {
        private const string Issuer = "TestIssuer";
        private const string Audience = "TestAudience";
        // A valid secret key: a Base64 string that decodes to at least 32 bytes.
        private const string ValidSecretKey = "VGhpcyBpcyBhIHZlcnkgc2VjdXJlIGtleSBmb3IgdGVzdGluZw==";
        private const int ExpirationHours = 1;

        /// <summary>
        /// Helper to create a mocked IConfigurationSection with a specified key and value.
        /// </summary>
        private static Mock<IConfigurationSection> SetupSection(string key, string value)
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s.Key).Returns(key);
            section.Setup(s => s.Value).Returns(value);
            return section;
        }

        /// <summary>
        /// Sets up a mock IConfiguration that provides sections for JWT settings.
        /// </summary>
        private IConfiguration SetupMockConfiguration(string issuer, string audience, string key, string expirationHours)
        {
            var mockConfig = new Mock<IConfiguration>();

            var issuerSection = SetupSection("JWT:Issuer", issuer);
            var audienceSection = SetupSection("JWT:Audience", audience);
            var keySection = SetupSection("JWT:Key", key);
            var expirationSection = SetupSection("JWT:ExpirationHours", expirationHours);

            // Setup GetSection calls (which are used by the GetValue<T>() extension method)
            mockConfig.Setup(c => c.GetSection("JWT:Issuer")).Returns(issuerSection.Object);
            mockConfig.Setup(c => c.GetSection("JWT:Audience")).Returns(audienceSection.Object);
            mockConfig.Setup(c => c.GetSection("JWT:Key")).Returns(keySection.Object);
            mockConfig.Setup(c => c.GetSection("JWT:ExpirationHours")).Returns(expirationSection.Object);

            // Optionally, also set up the indexer if needed.
            mockConfig.Setup(c => c["JWT:Issuer"]).Returns(issuer);
            mockConfig.Setup(c => c["JWT:Audience"]).Returns(audience);
            mockConfig.Setup(c => c["JWT:Key"]).Returns(key);
            mockConfig.Setup(c => c["JWT:ExpirationHours"]).Returns(expirationHours);

            return mockConfig.Object;
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidToken_WhenUserIsValid()
        {
            // Arrange
            IConfiguration configuration = SetupMockConfiguration(
                Issuer, 
                Audience, 
                ValidSecretKey, 
                ExpirationHours.ToString());
            var tokenGenerator = new JwtTokenGenerator(configuration);
            var user = new User { UserId = new Guid(), Email = "test@example.com", Role = UserRole.Admin };

            // Act
            var jwtToken = tokenGenerator.GenerateToken(user);

            // Assert
            Assert.NotNull(jwtToken);
            Assert.False(string.IsNullOrWhiteSpace(jwtToken.Token));
            Assert.Equal(Issuer, jwtToken.Issuer);
            Assert.True(jwtToken.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_ShouldThrowException_WhenConfigurationIsMissing()
        {
            // Arrange: simulate missing JWT:Key by returning a null value.
            var mockConfig = new Mock<IConfiguration>();

            var issuerSection = SetupSection("JWT:Issuer", Issuer);
            var audienceSection = SetupSection("JWT:Audience", Audience);
            var expirationSection = SetupSection("JWT:ExpirationHours", ExpirationHours.ToString());
            // JWT:Key is missing, so we set it up with a null value.
            var keySection = SetupSection("JWT:Key", null!);

            mockConfig.Setup(c => c.GetSection("JWT:Issuer")).Returns(issuerSection.Object);
            mockConfig.Setup(c => c.GetSection("JWT:Audience")).Returns(audienceSection.Object);
            mockConfig.Setup(c => c.GetSection("JWT:Key")).Returns(keySection.Object);
            mockConfig.Setup(c => c.GetSection("JWT:ExpirationHours")).Returns(expirationSection.Object);

            // Optionally, also set up the indexer if your implementation uses it.
            mockConfig.Setup(c => c["JWT:Issuer"]).Returns(Issuer);
            mockConfig.Setup(c => c["JWT:Audience"]).Returns(Audience);
            mockConfig.Setup(c => c["JWT:Key"]).Returns((string?)null);
            mockConfig.Setup(c => c["JWT:ExpirationHours"]).Returns(ExpirationHours.ToString());

            IConfiguration configuration = mockConfig.Object;
            var tokenGenerator = new JwtTokenGenerator(configuration);
            var user = new User { UserId = new Guid(), Email = "test@example.com", Role = UserRole.Admin };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => tokenGenerator.GenerateToken(user));
            Assert.Contains("Missing JWT settings", exception.Message);
        }

        [Fact]
        public void GenerateToken_ShouldThrowException_WhenKeyIsInvalid()
        {
            // Arrange: Provide a key that decodes to less than 32 bytes.
            string invalidKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("ShortKey"));
            IConfiguration configuration = SetupMockConfiguration(
                Issuer, 
                Audience, 
                invalidKey, 
                ExpirationHours.ToString());
            var tokenGenerator = new JwtTokenGenerator(configuration);
            var user = new User { UserId = new Guid(), Email = "test@example.com", Role = UserRole.Admin };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => tokenGenerator.GenerateToken(user));
            Assert.Equal("JWT key must be 256 bits (32 bytes) when decoded.", exception.Message);
        }

        [Fact]
        public void GenerateToken_ShouldContainCorrectClaims()
        {
            // Arrange
            IConfiguration configuration = SetupMockConfiguration(
                Issuer, 
                Audience, 
                ValidSecretKey, 
                ExpirationHours.ToString());
            var tokenGenerator = new JwtTokenGenerator(configuration);
            var user = new User { UserId = new Guid(), Email = "test@example.com", Role = UserRole.Admin };

            // Act
            var jwtToken = tokenGenerator.GenerateToken(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken.Token);

            // Assert: verify the token contains the expected claims
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.UserId.ToString());
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        }
    }
}