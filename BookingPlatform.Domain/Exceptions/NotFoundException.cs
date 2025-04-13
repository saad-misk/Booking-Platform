namespace BookingPlatform.Domain.Exceptions
{
    public class NotFoundException : CustomException
    {
        public NotFoundException(string message) 
            : base(message) { }
    }
}