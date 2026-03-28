using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CountCustomer()
        {
            return _context.Users.Count(u => u.Role.RoleName == "Customer");
        }

        public async Task<PagedResult<CustomerViewModel>> GetCustomersAsync(string? search, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "Customer")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(s)) ||
                    (u.Phone != null && u.Phone.Contains(s)) ||
                    (u.IDCard != null && u.IDCard.Contains(s)) ||
                    (u.Email != null && u.Email.ToLower().Contains(s)));
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new CustomerViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName ?? "(Chưa có tên)",
                    Gender = u.Gender,
                    IDCard = u.IDCard,
                    Address = u.Address,
                    Nationality = u.Nationality,
                    Email = u.Email,
                    Phone = u.Phone
                })
                .ToListAsync();

            return new PagedResult<CustomerViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CustomerViewModel?> GetCustomerById(int id)
        {
            var u = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (u == null) return null;

            return new CustomerViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName ?? "(Chưa có tên)",
                Gender = u.Gender,
                IDCard = u.IDCard,
                Address = u.Address,
                Nationality = u.Nationality,
                Email = u.Email,
                Phone = u.Phone
            };
        }
    }
}
