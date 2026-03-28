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

        public Task<int> CountRooms()
        {
            return _roomRepository.CountRoom();
        }

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, string? status, int page, int pageSize)
        {
            var result = await _roomRepository.GetAllRooms(search, status, page, pageSize);

            return new PagedResult<RoomViewModel>
            {
                Items = result.Items.Select(MapToViewModel).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<RoomViewModel> GetRoomById(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);
            return MapToViewModel(room);
        }

        public async Task<List<RoomViewModel>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(MapToViewModel).ToList();
        }

        public async Task<RoomViewModel?> GetByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
            {
                return null;
            }

            var model = MapToViewModel(room);
            model.RoomTypes = await _roomRepository.GetRoomTypesAsync();
            return model;
        }

        public async Task<RoomViewModel> CreateAsync(RoomViewModel model)
        {
            var room = new Room
            {
                RoomNumber = model.RoomNumber,
                Price = model.Price ?? 0m,
                Status = model.Status ?? "Available",
                RoomTypeName = model.RoomTypeName?.Trim() ?? string.Empty,
                Capacity = model.Capacity ?? 0,
                Description = model.Description,
                IsActive = model.IsActive
            };

            var created = await _roomRepository.CreateAsync(room);

            if (model.ImageUrls.Count > 0)
            {
                await _roomRepository.AddImagesAsync(created.RoomId, model.ImageUrls);
                created = await _roomRepository.GetRoomByIdAsync(created.RoomId);
            }

            return MapToViewModel(created);
        }

        public Task AddImagesAsync(int roomId, IEnumerable<string> urls)
        {
            return _roomRepository.AddImagesAsync(roomId, urls);
        }

        public Task<List<Image>> GetImagesByRoomIdAsync(int roomId)
        {
            return _roomRepository.GetImagesByRoomIdAsync(roomId);
        }

        public Task DeleteImagesAsync(int roomId, IEnumerable<int> imageIds)
        {
            return _roomRepository.DeleteImagesAsync(roomId, imageIds);
        }

        public async Task UpdateAsync(RoomViewModel model)
        {
            var room = await _roomRepository.GetByIdAsync(model.RoomId);
            if (room == null)
            {
                return;
            }

            room.RoomNumber = model.RoomNumber;
            room.Price = model.Price ?? room.Price;
            room.Status = model.Status ?? room.Status;
            room.RoomTypeName = model.RoomTypeName?.Trim() ?? room.RoomTypeName;
            room.Capacity = model.Capacity ?? room.Capacity;
            room.Description = model.Description;
            room.IsActive = model.IsActive;

            if (model.DeleteImageIds.Count > 0)
            {
                await _roomRepository.DeleteImagesAsync(room.RoomId, model.DeleteImageIds);
            }

            var newImageUrls = model.ImageUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct()
                .ToList();

            if (newImageUrls.Count > 0)
            {
                await _roomRepository.AddImagesAsync(room.RoomId, newImageUrls);
            }

            await _roomRepository.UpdateAsync(room);
        }

        public Task DeleteAsync(int id)
        {
            return _roomRepository.DeleteAsync(id);
        }

        public Task<List<RoomTypeItem>> GetRoomTypesAsync()
        {
            return _roomRepository.GetRoomTypesAsync();
        }

        private static RoomViewModel MapToViewModel(Room room)
        {
            return new RoomViewModel
            {
                RoomId = room.RoomId,
                RoomTypeId = null,
                RoomTypeName = room.RoomTypeName,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                Status = room.Status,
                Capacity = room.Capacity,
                Description = room.Description,
                IsActive = room.IsActive,
                ImageUrls = room.Images.Select(i => i.Url).ToList(),
                Images = room.Images.Select(i => new RoomImageItem
                {
                    ImageId = i.ImageId,
                    Url = i.Url
                }).ToList(),
                Feedbacks = room.Feedbacks
                    .OrderByDescending(f => f.FeedbackDate)
                    .Select(f => new FeedbackViewModel
                    {
                        FeedbackId = f.FeedbackId,
                        UserId = f.UserId,
                        Rating = f.Rating,
                        Comment = f.Comment,
                        FeedbackDate = f.FeedbackDate,
                        FullName = f.User?.FullName ?? f.User?.Username ?? "Khách hàng"
                    })
                    .ToList()
                    .ToList()
            };
        }
    }
}
