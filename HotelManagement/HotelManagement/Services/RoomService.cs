using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class RoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách phòng có phân trang.</summary>
        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var query = _context.Rooms
                .Include(r => r.Images)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r =>
                    r.RoomNumber.Contains(search) ||
                    r.RoomTypeName.Contains(search));

            int total = await query.CountAsync();

            var rooms = await query
                .OrderBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rooms.Select(r => new RoomViewModel
            {
                RoomId       = r.RoomId,
                RoomNumber   = r.RoomNumber,
                RoomTypeName = r.RoomTypeName,
                Price        = r.Price,
                Status       = r.Status,
                Capacity     = r.Capacity,
                Description  = r.Description,
                IsActive     = r.IsActive,
                ImageUrls    = r.Images.Select(i => i.Url).ToList()
            }).ToList();

            return new PagedResult<RoomViewModel>
            {
                Items     = items,
                TotalCount = total,
                Page      = page,
                PageSize  = pageSize
            };
        }

        /// <summary>Lấy chi tiết một phòng.</summary>
        public async Task<RoomViewModel?> GetRoomById(int roomId)
        {
            var r = await _context.Rooms
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (r == null) return null;

            return new RoomViewModel
            {
                RoomId       = r.RoomId,
                RoomNumber   = r.RoomNumber,
                RoomTypeName = r.RoomTypeName,
                Price        = r.Price,
                Status       = r.Status,
                Capacity     = r.Capacity,
                Description  = r.Description,
                IsActive     = r.IsActive,
                ImageUrls    = r.Images.Select(i => i.Url).ToList()
            };
        }
    }
}
