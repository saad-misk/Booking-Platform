using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Application.DTOs.Auth;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.JWT;
using BookingPlatform.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthenticationService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<AuthenticationService> logger,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponse> LoginAsync(
            string email, 
            string password,
            CancellationToken cancellationToken = default)
        {
            ValidateInput((email, nameof(email)), (password, nameof(password)));

            var user = await GetUserByEmailAsync(email, cancellationToken);
            ValidatePassword(user, password);

            var token = _jwtTokenGenerator.GenerateToken(user);
            var expiration = DateTime.UtcNow.AddHours(1);

            _logger.LogInformation("User {Email} logged in successfully", email);
            
            return new AuthResponse(Token: token, Expiration: expiration, Message: "Login successful");
        }

        public async Task<AuthResponse> RegisterAsync(
            string firstName,
            string lastName,
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            ValidateInput(
                (firstName, nameof(firstName)),
                (lastName, nameof(lastName)),
                (email, nameof(email)),
                (password, nameof(password))
            );

            await EnsureEmailIsUniqueAsync(email, cancellationToken);

            var user = CreateUser(firstName, lastName, email, password);
            await SaveUserAsync(user, cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(user);
            var expiration = DateTime.UtcNow.AddHours(1);

            _logger.LogInformation("User {Email} registered successfully", email);
            
           return new AuthResponse(Token: token, Expiration: expiration, Message: "Registration successful");
        }

        // Helper methods
        private async Task<User> GetUserByEmailAsync(
            string email, 
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
            return user ?? throw new UserNotFoundException();
        }

        private async Task EnsureEmailIsUniqueAsync(
            string email,
            CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.FindByEmailAsync(email, cancellationToken);
            if (existingUser != null) throw new DuplicateEmailException();
        }

        private async Task SaveUserAsync(
            User user,
            CancellationToken cancellationToken)
        {
            var success = await _userRepository.CreateUserAsync(user, cancellationToken);
            if (!success) throw new UserRegistrationException();
        }

        private void ValidateInput(params (string Value, string FieldName)[] inputs)
        {
            foreach (var (value, fieldName) in inputs)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException($"{fieldName} is required");
            }
        }

        private void ValidatePassword(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user, 
                user.PasswordHash,
                password
            );
            
            if (result != PasswordVerificationResult.Success)
                throw new InvalidPasswordException();
        }

        private User CreateUser(
            string firstName, 
            string lastName, 
            string email, 
            string password)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Role = UserRole.NormalUser,
                Email = email,
                UserName = email,
                PasswordHash = _passwordHasher.HashPassword(new User(), password)
            };
        }
    }
}