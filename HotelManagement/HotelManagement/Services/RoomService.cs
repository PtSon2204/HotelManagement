using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class RoomService
    {
        private readonly RoomRepository _repo;

        public RoomService(RoomRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> CountRooms() => await _repo.CountRooms();

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
            => await _repo.GetAllRoomsAsync(search, page, pageSize);

        public async Task<Room?> GetRoomById(int id)
            => await _repo.GetRoomById(id);

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, string? status, int page, int pageSize)
        {
            var result = await _repo.GetAllRooms(search, status, page, pageSize);

            return new PagedResult<RoomViewModel>
            {
                Items = result.Items.Select(MapToViewModel).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
        public async Task<List<RoomViewModel>> GetAllAsync()
        {
            var rooms = await _repo.GetAllAsync();
            return rooms.Select(MapToViewModel).ToList();
        }

        public async Task<RoomViewModel?> GetByIdAsync(int id)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null)
            {
                return null;
            }

            var model = MapToViewModel(room);
            model.RoomTypes = await _repo.GetRoomTypesAsync();
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

            var created = await _repo.CreateAsync(room);

            if (model.ImageUrls.Count > 0)
            {
                await _repo.AddImagesAsync(created.RoomId, model.ImageUrls);
                created = await _repo.GetRoomByIdAsync(created.RoomId);
            }

            return MapToViewModel(created);
        }

        public Task AddImagesAsync(int roomId, IEnumerable<string> urls)
        {
            return _repo.AddImagesAsync(roomId, urls);
        }

        public Task<List<Image>> GetImagesByRoomIdAsync(int roomId)
        {
            return _repo.GetImagesByRoomIdAsync(roomId);
        }

        public Task DeleteImagesAsync(int roomId, IEnumerable<int> imageIds)
        {
            return _repo.DeleteImagesAsync(roomId, imageIds);
        }

        public async Task UpdateAsync(RoomViewModel model)
        {
            var room = await _repo.GetByIdAsync(model.RoomId);
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
                await _repo.DeleteImagesAsync(room.RoomId, model.DeleteImageIds);
            }

            var newImageUrls = model.ImageUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct()
                .ToList();

            if (newImageUrls.Count > 0)
            {
                await _repo.AddImagesAsync(room.RoomId, newImageUrls);
            }

            await _repo.UpdateAsync(room);
        }

        public Task DeleteAsync(int id)
        {
            return _repo.DeleteAsync(id);
        }

        public Task<List<RoomTypeItem>> GetRoomTypesAsync()
        {
            return _repo.GetRoomTypesAsync();
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
            };
        }
    }
}
