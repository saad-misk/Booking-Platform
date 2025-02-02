using System.ComponentModel.DataAnnotations;
using BookingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BookingPlatform.Application.DTOs.Images.Requests
{
    public class AddImage
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public ImageEntityType EntityType { get; set; }

        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}