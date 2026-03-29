using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class ServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CountService()
        {
            return _context.Services.Count(s => s.IsActive == true);
        }

        public async Task<List<Service>> GetAllAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        /// <summary>Lấy toàn bộ service gồm cả phụ phí (IsActive = false).</summary>
        public async Task<List<Service>> GetAllIncludingPenaltyAsync()
        {
            return await _context.Services
                .OrderBy(s => s.IsActive)   // phụ phí (false/0) lên trước
                .ThenBy(s => s.Name)
                .ToListAsync();
        }
    }
}
