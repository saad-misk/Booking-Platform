using BookingPlatform.Application.DTOs.Search;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Hotels.Requests
{
    public class HotelSearchCriteria : PaginationCriteria
    {
        public string? Name { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int StarRating { get; set; }
        public RoomType RoomType { get; set; }
        public Guid? CityId { get; set; }
        public double? Rating { get; set; }
    }
}