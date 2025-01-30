namespace BookingPlatform.Domain.Entities
{
    public class City
    {
        public Guid CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostOffice { get; set; } = string.Empty;
        public string CityCode { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
        public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
        public ICollection<Image> Images { get; set; } = new List<Image>();
    } 
}