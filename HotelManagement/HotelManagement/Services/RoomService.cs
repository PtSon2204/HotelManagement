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
    }
}
