using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class RoomService
    {
        private readonly RoomRepository _roomRepository;

        public RoomService(RoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<List<RoomViewModel>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(r => new RoomViewModel
            {
                RoomId = r.RoomId,
                RoomTypeId = r.RoomTypeId,
                RoomNumber = r.RoomNumber,
                Image = r.Image,
                Price = r.Price,
                Status = r.Status,
                RoomTypeName = r.RoomType?.Name
            }).ToList();
        }

        public async Task<RoomViewModel?> GetByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return null;

            var roomTypes = await _roomRepository.GetRoomTypesAsync();
            return new RoomViewModel
            {
                RoomId = room.RoomId,
                RoomTypeId = room.RoomTypeId,
                RoomNumber = room.RoomNumber,
                Image = room.Image,
                Price = room.Price,
                Status = room.Status,
                RoomTypeName = room.RoomType?.Name,
                RoomTypes = roomTypes.Select(rt => new RoomTypeItem
                {
                    RoomTypeId = rt.RoomTypeId,
                    Name = rt.Name,
                    Price = rt.Price
                }).ToList()
            };
        }

        public async Task<RoomViewModel> CreateAsync(RoomViewModel model)
        {
            var room = new Room
            {
                RoomTypeId = model.RoomTypeId,
                RoomNumber = model.RoomNumber,
                Image = model.Image,
                Price = model.Price,
                Status = model.Status ?? "Available"
            };

            var created = await _roomRepository.CreateAsync(room);
            return new RoomViewModel
            {
                RoomId = created.RoomId,
                RoomTypeId = created.RoomTypeId,
                RoomNumber = created.RoomNumber,
                Image = created.Image,
                Price = created.Price,
                Status = created.Status
            };
        }

        public async Task UpdateAsync(RoomViewModel model)
        {
            var room = await _roomRepository.GetByIdAsync(model.RoomId);
            if (room == null) return;

            room.RoomTypeId = model.RoomTypeId;
            room.RoomNumber = model.RoomNumber;
            room.Image = model.Image;
            room.Price = model.Price;
            room.Status = model.Status;

            await _roomRepository.UpdateAsync(room);
        }

        public async Task DeleteAsync(int id)
        {
            await _roomRepository.DeleteAsync(id);
        }

        public async Task<List<RoomTypeItem>> GetRoomTypesAsync()
        {
            var roomTypes = await _roomRepository.GetRoomTypesAsync();
            return roomTypes.Select(rt => new RoomTypeItem
            {
                RoomTypeId = rt.RoomTypeId,
                Name = rt.Name,
                Price = rt.Price
            }).ToList();
        }
    }
}
