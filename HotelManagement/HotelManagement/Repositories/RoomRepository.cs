using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class RoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountRooms()
        {
            return await _context.Rooms.CountAsync(r => r.IsActive);
        }

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var query = _context.Rooms
                .Include(r => r.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(r =>
                    r.RoomNumber.ToLower().Contains(s) ||
                    r.RoomTypeName.ToLower().Contains(s) ||
                    r.Status.ToLower().Contains(s));
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomViewModel
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    Price = r.Price,
                    Status = r.Status,
                    RoomTypeName = r.RoomTypeName,
                    Capacity = r.Capacity,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    Images = r.Images
                        .Select(i => new RoomImageItem { ImageId = i.ImageId, Url = i.Url })
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<RoomViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Room?> GetRoomById(int id)
        {
            return await _context.Rooms
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }
    }
}
