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

        public async Task<int> CountRooms() => await _roomRepository.CountRoom();

        private RoomViewModel ToViewModel(Room r)
        {
            return new RoomViewModel
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                Price = r.Price,
                Status = r.Status,
                RoomTypeName = r.RoomTypeName,
                Capacity = r.Capacity,
                Description = r.Description,
                IsActive = r.IsActive,
                ImageUrls = r.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
                Images = r.Images?.Select(i => new RoomImageItem
                {
                    ImageId = i.ImageId,
                    Url = i.Url
                }).ToList() ?? new List<RoomImageItem>()
            };
        }

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var result = await _roomRepository.GetAllRooms(search, page, pageSize);

            return new PagedResult<RoomViewModel>
            {
                Items = result.Items.Select(x => ToViewModel(x)).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<RoomViewModel> GetRoomById(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);
            if (room == null) throw new Exception("Room not found");
            return ToViewModel(room);
        }

        public async Task<List<RoomViewModel>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(r => ToViewModel(r)).ToList();
        }

        public async Task<RoomViewModel?> GetByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return null;
            return ToViewModel(room);
        }

        public async Task<RoomViewModel> CreateAsync(RoomViewModel model)
        {
            var room = new Room
            {
                RoomNumber = model.RoomNumber,
                Price = model.Price ?? 0,
                Status = model.Status ?? "Available",
                RoomTypeName = model.RoomTypeName ?? string.Empty,
                Capacity = model.Capacity ?? 0,
                Description = model.Description ?? string.Empty,
                IsActive = model.IsActive,
                Images = model.ImageUrls?.Select(url => new Image { Url = url }).ToList() ?? new List<Image>()
            };

            var created = await _roomRepository.CreateAsync(room);
            return ToViewModel(created);
        }

        public async Task AddImagesAsync(int roomId, IEnumerable<string> urls)
        {
            await _roomRepository.AddImagesAsync(roomId, urls);
        }

        public async Task<List<Image>> GetImagesByRoomIdAsync(int roomId)
        {
            return await _roomRepository.GetImagesByRoomIdAsync(roomId);
        }

        public async Task DeleteImagesAsync(int roomId, IEnumerable<int> imageIds)
        {
            await _roomRepository.DeleteImagesAsync(roomId, imageIds);
        }

        public async Task UpdateAsync(RoomViewModel model)
        {
            var room = await _roomRepository.GetByIdAsync(model.RoomId);
            if (room == null) return;

            room.RoomNumber = model.RoomNumber;
            room.Price = model.Price ?? room.Price;
            room.Status = model.Status ?? room.Status;
            room.RoomTypeName = model.RoomTypeName ?? room.RoomTypeName;
            room.Capacity = model.Capacity ?? room.Capacity;
            room.Description = model.Description ?? room.Description;
            room.IsActive = model.IsActive;

            // Xóa ảnh
            if (model.DeleteImageIds != null && model.DeleteImageIds.Any())
            {
                await DeleteImagesAsync(room.RoomId, model.DeleteImageIds);
            }

            // Thêm ảnh mới
            if (model.ImageUrls != null && model.ImageUrls.Any())
            {
                foreach (var url in model.ImageUrls)
                {
                    room.Images.Add(new Image { Url = url, RoomId = room.RoomId });
                }
            }

            await _roomRepository.UpdateAsync(room);
        }

        public async Task DeleteAsync(int id)
        {
            await _roomRepository.DeleteAsync(id);
        }
    }
}