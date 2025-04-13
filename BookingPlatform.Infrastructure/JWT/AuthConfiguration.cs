using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using BookingPlatform.Domain.Interfaces.JWT;

namespace BookingPlatform.Infrastructure.JWT
{
    public static class AuthConfiguration
    {
        public static IServiceCollection AddAuthInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration
        )
        {
            var issuer = configuration["JWT:Issuer"];
            var audience = configuration["JWT:Audience"];
            var key = configuration["JWT:Key"];

            // Validate configuration presence
            var missingSettings = new List<string>();
            if (string.IsNullOrEmpty(issuer)) missingSettings.Add("Issuer");
            if (string.IsNullOrEmpty(audience)) missingSettings.Add("Audience");
            if (string.IsNullOrEmpty(key)) missingSettings.Add("Key");
            if (missingSettings.Any())
                throw new InvalidOperationException(
                    $"Missing JWT settings: {string.Join(", ", missingSettings)}"
                );

            // Decode and validate key length
            var keyBytes = Convert.FromBase64String(key!);
            if (keyBytes.Length < 32)
                throw new InvalidOperationException("JWT key must be 256 bits (32 bytes).");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
            return services;
        }
    }
}