namespace BookingPlatform.Domain.Exceptions
{
    public class BadRequestException : CustomException
    {
        public BadRequestException(string message) 
            : base(message) { }
    }
}