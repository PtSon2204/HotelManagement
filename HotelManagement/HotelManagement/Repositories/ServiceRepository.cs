using System.Reflection.Metadata.Ecma335;
using HotelManagement.Context;
using HotelManagement.Models.Common;
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

        public int CountService => _context.Services.Count();
        
        public async Task<List<Service>> GetServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }

    }
}
