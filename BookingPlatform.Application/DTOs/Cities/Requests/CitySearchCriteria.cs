using System.ComponentModel.DataAnnotations;
using BookingPlatform.Application.DTOs.Search;

namespace BookingPlatform.Application.DTOs.Cities.Requests
{
    public class CitySearchCriteria : PaginationCriteria
    {
        [Required]
        public string Name { get; set; }
    }
}