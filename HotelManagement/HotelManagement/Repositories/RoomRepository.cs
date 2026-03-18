using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
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
        public int CountRoom() => _context.Rooms.Count();
        public async Task<PagedResult<Room>> GetAllRooms(string? search, int page, int pageSize)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Status.Contains(search));
            }

            int totalCount = await query.CountAsync();

            var items = await query.OrderBy(x => x.RoomId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Room>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms.Include(x => x.RoomType)
                                       .FirstOrDefaultAsync(x => x.RoomId == id); 
        }
    }
}
