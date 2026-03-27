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

        public async Task<int> CountRoom()
        {
            var list = await _context.Rooms.AsNoTracking().ToListAsync();
            return list.Count(x => x.Status == "Available");
        }

        public async Task<PagedResult<Room>> GetAllRooms(string? search, int page, int pageSize)
        {
            var query = _context.Rooms
                .Include(x => x.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Status.Contains(search));
            }

            query = query.OrderBy(x =>
                x.Status == "Available" ? 1 :
                x.Status == "Occupied" ? 2 :
                x.Status == "Maintenance" ? 3 : 4
            );

            int totalCount = await query.CountAsync();

            var items = await query
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
            return await _context.Rooms
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.RoomId == id);
        }

        public async Task<List<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .Include(r => r.Images)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task AddImagesAsync(int roomId, IEnumerable<string> urls)
        {
            var imageEntities = urls
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => new Image { RoomId = roomId, Url = u.Trim() })
                .ToList();

            if (imageEntities.Count == 0) return;

            await _context.Images.AddRangeAsync(imageEntities);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Image>> GetImagesByRoomIdAsync(int roomId)
        {
            return await _context.Images
                .Where(i => i.RoomId == roomId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task DeleteImagesAsync(int roomId, IEnumerable<int> imageIds)
        {
            var ids = imageIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0) return;

            var toDelete = await _context.Images
                .Where(i => i.RoomId == roomId && ids.Contains(i.ImageId))
                .ToListAsync();

            if (toDelete.Count == 0) return;

            _context.Images.RemoveRange(toDelete);
            await _context.SaveChangesAsync();
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
    }
}