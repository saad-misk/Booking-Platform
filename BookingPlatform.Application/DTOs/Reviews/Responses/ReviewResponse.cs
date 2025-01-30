namespace BookingPlatform.Application.DTOs.Reviews.Responses
{
    public class ReviewResponse
    {
        public string Author { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }
    }
}