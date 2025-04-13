namespace BookingPlatform.Application.DTOs.Search
{
    public class PaginationCriteria
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}