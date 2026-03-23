using HotelManagement.Models.Common;
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

        public int CountRooms() => _roomRepository.CountRoom();

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var result = await _roomRepository.GetAllRooms(search, page, pageSize);

            return new PagedResult<RoomViewModel>
            {
                Items = result.Items.Select(x => new RoomViewModel
                {
                    RoomId = x.RoomId,
                    RoomTypeId = x.RoomTypeId,
                    RoomNumber = x.RoomNumber,
                    Price = x.Price,
                    Status = x.Status,
                    RoomTypeName = x.RoomType.Name,

                    ImageUrls = x.Images.Select(i => i.Url).ToList()
                }).ToList(),

                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<RoomViewModel> GetRoomById(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            return new RoomViewModel
            {
                RoomId = room.RoomId,
                Price = room.Price,
                RoomNumber = room.RoomNumber,
                Status = room.Status,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType?.Name,
                Description = room.RoomType?.Description,

                ImageUrls = room.Images.Select(i => i.Url).ToList()
            };
        }

        public async Task<List<RoomViewModel>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();

            return rooms.Select(r => new RoomViewModel
            {
                RoomId = r.RoomId,
                RoomTypeId = r.RoomTypeId,
                RoomNumber = r.RoomNumber,
                Price = r.Price,
                Status = r.Status,
                RoomTypeName = r.RoomType?.Name,
                Description = r.RoomType?.Description,

                ImageUrls = r.Images.Select(i => i.Url).ToList()
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
                Price = room.Price,
                Status = room.Status,
                RoomTypeName = room.RoomType?.Name,
                Description = room.RoomType?.Description,

                ImageUrls = room.Images.Select(i => i.Url).ToList(),

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
                Price = model.Price,
                Status = model.Status ?? "Available",

                // ✅ lưu nhiều ảnh
                Images = model.ImageUrls.Select(url => new Image
                {
                    Url = url
                }).ToList()
            };

            var created = await _roomRepository.CreateAsync(room);

            return new RoomViewModel
            {
                RoomId = created.RoomId,
                RoomTypeId = created.RoomTypeId,
                RoomNumber = created.RoomNumber,
                Price = created.Price,
                Status = created.Status,
                ImageUrls = created.Images.Select(i => i.Url).ToList()
            };
        }

        public async Task UpdateAsync(RoomViewModel model)
        {
            var room = await _roomRepository.GetByIdAsync(model.RoomId);
            if (room == null) return;

            room.RoomTypeId = model.RoomTypeId;
            room.RoomNumber = model.RoomNumber;
            room.Price = model.Price;
            room.Status = model.Status;

            // ✅ update lại image (simple version: xóa hết rồi thêm lại)
            room.Images.Clear();

            foreach (var url in model.ImageUrls)
            {
                room.Images.Add(new Image
                {
                    Url = url,
                    RoomId = room.RoomId
                });
            }

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