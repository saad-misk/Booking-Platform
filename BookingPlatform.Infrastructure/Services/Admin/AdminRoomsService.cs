using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;

namespace BookingPlatform.Infrastructure.Services.Admin
{
    public class AdminRoomsService : IAdminRoomsService
    {
        IRoomsRepository _roomRepository;
        IUnitOfWork _unitOfWork;
        
        public AdminRoomsService(IRoomsRepository roomRepository, IUnitOfWork unitOfWork)
        {
            _roomRepository = roomRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<List<RoomResponse>> GetAllRoomsAsync()
        {
            var rooms = (List<Room>)await _roomRepository.GetAsync();
            ICollection<RoomResponse> responseList = new List<RoomResponse>();
            foreach(Room room in rooms)
                responseList.Add(MapToRoomResponse(room));
            return (List<RoomResponse>)responseList;
        }

        public async Task<Room> GetRoomByIdAsync(Guid roomId)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);
            return room!;
        }

        public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest room)
        {
            var newRoom = new Room
            {
                RoomId = Guid.NewGuid(),
                RoomClass = room.RoomType,
                PricePerNight = room.PricePerNight,
                Capacity = room.Capacity,
                Number = room.Number
            };
            await _roomRepository.AddAsync(newRoom);
            await _unitOfWork.CommitAsync();
            return MapToRoomResponse(newRoom);
        }

        public async Task UpdateRoomAsync(UpdateRoomRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId) 
                ?? throw new NotFoundException("Room not found");

            UpdateRoomProperties(room, request);
            
            _roomRepository.Update(room);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteRoomAsync(Guid roomId)
        {
            var room = await _roomRepository.GetByIdAsync(roomId)
                ?? throw new NotFoundException("Room not found");
            
            await _roomRepository.DeleteAsync(room.RoomId);
            await _unitOfWork.CommitAsync();
        }

        private void UpdateRoomProperties(Room room, UpdateRoomRequest request)
        {
            if (request.RoomType.HasValue)
            {
                room.RoomClass = ValidateRoomType((int)request.RoomType.Value);
            }
            
            if (request.PricePerNight.HasValue)
            {
                ValidatePositive(request.PricePerNight.Value, nameof(request.PricePerNight));
                room.PricePerNight = request.PricePerNight.Value;
            }
            
            if (request.Capacity.HasValue)
            {
                ValidatePositive(request.Capacity.Value, nameof(request.Capacity));
                room.Capacity = request.Capacity.Value;
            }
        }

        private static RoomType ValidateRoomType(int roomType)
        {
            if (!Enum.IsDefined(typeof(RoomType), roomType))
            {
                throw new BadRequestException($"Invalid room type value: {roomType}");
            }
            return (RoomType)roomType;
        }

        private static void ValidatePositive<T>(T value, string propertyName) where T : struct, IComparable<T>
        {
            if (value.CompareTo(default(T)) <= 0)
            {
                throw new BadRequestException($"{propertyName} must be positive");
            }
        }
        private RoomResponse MapToRoomResponse(Room room)
        {
            return new RoomResponse
            {
                RoomId = room.RoomId,
                RoomType = room.RoomClass,
                PricePerNight = room.PricePerNight,
                Status = room.Status,
                Capacity = room.Capacity,
            };
        }

    }
}