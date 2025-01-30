namespace BookingPlatform.Application.DTOs.Search
{
    public class SearchResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}