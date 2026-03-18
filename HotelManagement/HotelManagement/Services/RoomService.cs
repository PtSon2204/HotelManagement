using HotelManagement.Models.Common;
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

        public int CountRooms() => _repo.CountRoom();

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var result = await _repo.GetAllRooms(search, page, pageSize);

            return new PagedResult<RoomViewModel>
            {
                Items = result.Items.Select(x => new RoomViewModel
                {
                    RoomId = x.RoomId,
                    RoomTypeId = x.RoomTypeId,
                    Image = x.Image,
                    Price = x.Price,
                    RoomNumber = x.RoomNumber,
                    Status = x.Status
                }).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<RoomViewModel> GetRoomById(int id)
        {
            var room = await _repo.GetRoomByIdAsync(id);

            return new RoomViewModel
            {
                RoomId = room.RoomId,
                Price = room.Price,
                RoomNumber = room.RoomNumber,
                Status = room.Status,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType?.Name
            };
        }
        
    }
}
