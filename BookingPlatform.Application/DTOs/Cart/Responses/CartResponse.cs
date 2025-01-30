namespace BookingPlatform.Application.DTOs.Cart.Responses
{
    public class CartResponse
    {
        public List<CartItemResponse> Items { get; set; }
        public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
    }
}