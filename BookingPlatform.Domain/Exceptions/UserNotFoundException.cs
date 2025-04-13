namespace BookingPlatform.Domain.Exceptions
{
    public sealed class UserNotFoundException : CustomException
    {
        public UserNotFoundException() 
            : base("User not found. Invalid credentials.") { }
    }
}