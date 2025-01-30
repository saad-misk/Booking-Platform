using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminRoomsService : IAdminService
    {
        Task UpdateRoomAsync(UpdateRoomRequest request);
        Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request);
        Task DeleteRoomAsync(Guid roomId);
        Task<List<RoomResponse>> GetAllRoomsAsync();
        Task<Room> GetRoomByIdAsync(Guid roomId);
    }
}