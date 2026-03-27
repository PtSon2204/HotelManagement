using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class CustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CountCustomer()
        {
            return _context.Users.Count(u => u.Role != null && u.Role.RoleName == "Customer");
        }

        public async Task<PagedResult<User>> GetCustomersAsync(string? search, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "Customer")
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    (x.Phone != null && x.Phone.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<User?> GetCustomerById(int id)
        {
            return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}
