using BookingPlatform.Domain.Interfaces.JWT;
using BookingPlatform.Domain.Models;
using BookingPlatform.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BookingPlatform.Infrastructure.JWT
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public JwtToken GenerateToken(User user)
        {
            // Fetch configuration
            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];
            var key = _configuration["JWT:Key"];
            var expirationHours = _configuration.GetValue<int>("JWT:ExpirationHours");

            // Validate configuration
            var missingSettings = new List<string>();
            if (string.IsNullOrEmpty(issuer)) missingSettings.Add("Issuer");
            if (string.IsNullOrEmpty(audience)) missingSettings.Add("Audience");
            if (string.IsNullOrEmpty(key)) missingSettings.Add("Key");
            if (missingSettings.Any())
                throw new InvalidOperationException(
                    $"Missing JWT settings: {string.Join(", ", missingSettings)}"
                );

            // Decode and validate key
            var keyBytes = Convert.FromBase64String(key!);
            if (keyBytes.Length < 32)
                throw new InvalidOperationException("JWT key must be 256 bits (32 bytes) when decoded.");

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Generate token
            var symmetricKey = new SymmetricSecurityKey(keyBytes);
            var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            var expirationTime = DateTime.UtcNow.AddHours(expirationHours);

            var jwtToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expirationTime,
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(jwtToken);

            return new JwtToken(token, expirationTime, issuer!);
        }
    }
}