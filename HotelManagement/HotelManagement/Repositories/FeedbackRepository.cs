using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class FeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public FeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int CountFeedback() => _context.Feedbacks.Count();

        public async Task<PagedResult<Feedback>> GetAllFeedbacks(string? search, int page, int pageSize)
        {
            var query = _context.Feedbacks.Include(x => x.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(search) )
            {
                query = query.Where(x => x.Customer.FullName.Contains(search));
            }

            int totalCount = await query.CountAsync();
            var items = await query.OrderBy(x => x.FeedbackId)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .AsNoTracking()
                              .ToListAsync();

            return new PagedResult<Feedback>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Feedback?> GetFeedbackById(int id)
        {
            return await _context.Feedbacks.Include(x => x.Customer).FirstOrDefaultAsync(x => x.FeedbackId == id);
        }
    }
}
