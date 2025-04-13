using BookingPlatform.Application.DTOs.Search;

namespace BookingPlatform.Application.DTOs.Rooms.Requests
{
    public class RoomSearchCriteria : PaginationCriteria
    {
        public string? Name { get; set; }
        public Guid? HotelId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

    }
}