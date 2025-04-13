namespace BookingPlatform.Application.DTOs.Reviews.Responses
{
    public class ReviewResponse
    {
        public string Author { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
    }
}