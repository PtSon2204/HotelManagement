using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class RoomTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomType>> GetAllRoomTypes()
        {
            return await _context.RoomTypes
                .Where(r => r.IsActive == true)
                .ToListAsync();
        }

        public async Task<RoomType?> GetRoomTypeById(int id)
        {
            return await _context.RoomTypes
                .FirstOrDefaultAsync(r => r.RoomTypeId == id);
        }
    }
}
