namespace BookingPlatform.Application.DTOs.Cart.Responses
{
    public class CartResponse
    {
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
    }
}