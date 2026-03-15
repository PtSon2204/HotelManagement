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
        public int CountCustomer() => _context.Customers.Count();
        public async Task<PagedResult<Customer>> GetCustomersAsync(string? search, int page, int pageSize)
        {
            var query = _context.Customers.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Phone.Contains(search) ||
                    x.Email.Contains(search));
            }

            // Total record
            int totalCount = await query.CountAsync();

            // Paging
            var items = await query
                .OrderBy(x => x.CustomerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Customer?> GetCustomerById(int id)
        {
            return await _context.Customers.FindAsync(id);
        }


    }
}
