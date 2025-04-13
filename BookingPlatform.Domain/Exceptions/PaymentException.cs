namespace BookingPlatform.Domain.Exceptions
{
    public class PaymentException: CustomException
    {
        public PaymentException(string message) 
            : base(message) { }
    }
}