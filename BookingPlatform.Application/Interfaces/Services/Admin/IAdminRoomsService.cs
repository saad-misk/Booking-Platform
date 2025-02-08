using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminRoomsService : IAdminService
    {
        Task UpdateRoomAsync(UpdateRoomRequest request, CancellationToken cancellationToken = default);
        Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
        Task DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<List<RoomResponse>> GetAllRoomsAsync(CancellationToken cancellationToken = default);
        Task<Room> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    }
}