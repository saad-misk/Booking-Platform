using System.ComponentModel.DataAnnotations;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Images.Requests
{
    public class DeleteImageRequest
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public ImageEntityType EntityType { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}