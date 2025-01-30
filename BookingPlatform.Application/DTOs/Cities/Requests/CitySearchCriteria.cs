using BookingPlatform.Application.DTOs.Search;

namespace BookingPlatform.Application.DTOs.Cities.Requests
{
    public class CitySearchCriteria : PaginationCriteria
    {
        public string Name { get; set; }
    }
}