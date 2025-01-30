namespace BookingPlatform.Domain.Entities
{
    public class Hotel
    {
        public Guid HotelId { get; set; }
        public Guid CityId { get; set; }
        public City City { get; set; }
        public ICollection<Image> Gallery { get; set; } = new List<Image>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public string Name { get; set; } = string.Empty;
        public double ReviewsRating { get; set; }
        public int StarRating { get; set; }
        public double Longitude { get; set; } // for determining location on map
        public double Latitude { get; set; }
        public string? Description { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }
}