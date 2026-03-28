using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
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

        public int CountFeedback()
        {
            return _context.Feedbacks.Count();
        }

        public async Task<PagedResult<FeedbackViewModel>> GetAllFeedback(string? search, int page, int pageSize)
        {
            var query = _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Room)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(f =>
                    (f.User.FullName != null && f.User.FullName.ToLower().Contains(s)) ||
                    f.Room.RoomNumber.ToLower().Contains(s) ||
                    (f.Comment != null && f.Comment.ToLower().Contains(s)));
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.FeedbackDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FeedbackViewModel
                {
                    FeedbackId = f.FeedbackId,
                    UserId = f.UserId,
                    FullName = f.User.FullName ?? "(Không tên)",
                    RoomId = f.RoomId,
                    RoomNumber = f.Room.RoomNumber,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    FeedbackDate = f.FeedbackDate
                })
                .ToListAsync();

            return new PagedResult<FeedbackViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<FeedbackViewModel?> GetFeedbackById(int id)
        {
            var f = await _context.Feedbacks
                .Include(x => x.User)
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.FeedbackId == id);

            if (f == null) return null;

            return new FeedbackViewModel
            {
                FeedbackId = f.FeedbackId,
                UserId = f.UserId,
                FullName = f.User?.FullName ?? "(Không tên)",
                RoomId = f.RoomId,
                RoomNumber = f.Room?.RoomNumber,
                Rating = f.Rating,
                Comment = f.Comment,
                FeedbackDate = f.FeedbackDate
            };
        }
    }
}
