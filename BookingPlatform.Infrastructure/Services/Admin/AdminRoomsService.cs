using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;

namespace BookingPlatform.Infrastructure.Services.Admin
{
    public class AdminRoomsService : IAdminRoomsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminRoomsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<RoomResponse>> GetAllRoomsAsync(CancellationToken cancellationToken = default)
        {
            var rooms = await _unitOfWork.GetRepository<Room>().GetAsync(cancellationToken: cancellationToken);
            var responseList = rooms.Select(MapToRoomResponse).ToList();
            return responseList;
        }

        public async Task<Room> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            var room = await _unitOfWork.GetRepository<Room>().GetByIdAsync(roomId, cancellationToken)
                ?? throw new NotFoundException("Room not found");

            return room;
        }

        public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
        {
            var hotel = await _unitOfWork.GetRepository<Hotel>().GetByIdAsync(request.HotelId)
                ?? throw new NotFoundException("Hotel not found!");

            var newRoom = new Room
            {
                RoomId = Guid.NewGuid(),
                Hotel = hotel,
                HotelId = hotel.HotelId,
                RoomClass = request.RoomType,
                PricePerNight = request.PricePerNight,
                Capacity = request.Capacity,
                Number = request.Number
            };

            await _unitOfWork.GetRepository<Room>().AddAsync(newRoom, cancellationToken);
            await _unitOfWork.CommitAsync();

            return MapToRoomResponse(newRoom);
        }

        public async Task UpdateRoomAsync(UpdateRoomRequest request, CancellationToken cancellationToken = default)
        {
            var room = await _unitOfWork.GetRepository<Room>().GetByIdAsync(request.RoomId, cancellationToken)
                ?? throw new NotFoundException("Room not found");

            UpdateRoomProperties(room, request);

            _unitOfWork.GetRepository<Room>().Update(room);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            var room = await _unitOfWork.GetRepository<Room>().GetByIdAsync(roomId, cancellationToken)
                ?? throw new NotFoundException("Room not found");

            await _unitOfWork.GetRepository<Room>().DeleteAsync(room.RoomId, cancellationToken);
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
            if (value.CompareTo(default) <= 0)
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