namespace BookingPlatform.Domain.Entities
{
    public class Image
    {
        public Guid ImageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsThumbnail { get; set; }
        public string Url { get; set; } = string.Empty;
        public Guid? HotelId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? RoomId { get; set; }
        public Hotel? Hotel { get; set; }
        public City? City { get; set; }
        public Room? Room { get; set; }
    }
}