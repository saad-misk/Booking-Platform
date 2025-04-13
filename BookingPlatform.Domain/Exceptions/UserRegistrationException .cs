namespace BookingPlatform.Domain.Exceptions
{
    public sealed class UserRegistrationException : CustomException
    {
        public UserRegistrationException() 
            : base("Failed to register user.") { }
    }
}