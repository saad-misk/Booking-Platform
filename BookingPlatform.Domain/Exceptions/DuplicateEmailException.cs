namespace BookingPlatform.Domain.Exceptions
{
    public sealed class DuplicateEmailException : CustomException
    {
        public DuplicateEmailException() 
            : base("Email is already registered.") { }
    }
}