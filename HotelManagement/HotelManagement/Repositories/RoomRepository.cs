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
        public async Task<List<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task<Room> CreateAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RoomType>> GetRoomTypesAsync()
        {
            return await _context.RoomTypes.Where(rt => rt.IsActive == true).ToListAsync();
        }
    }
}
