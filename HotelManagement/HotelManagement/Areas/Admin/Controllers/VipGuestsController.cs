using HotelManagement.Context;
using HotelManagement.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class VipGuestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VipGuestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sort = "totalSpent", int top = 10)
        {
            // Aggregate bookings grouped by user
            var bookingData = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Invoice)
                .Where(b => b.User != null)
                .GroupBy(b => new
                {
                    b.UserId,
                    b.User.FullName,
                    b.User.Phone,
                    b.User.Email,
                    b.User.Nationality
                })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.FullName,
                    g.Key.Phone,
                    g.Key.Email,
                    g.Key.Nationality,
                    TotalSpent = g.Where(b => b.Invoice != null && b.Invoice.Status == "Paid")
                                  .Sum(b => (decimal?)b.Invoice!.TotalAmount) ?? 0m,
                    TotalStays = g.Count(b => b.Status == "CheckedOut"),
                    TotalBookings = g.Count(),
                    LastStay = g.Where(b => b.ActualCheckOut.HasValue)
                                .Max(b => (DateTime?)b.ActualCheckOut)
                })
                .ToListAsync();

            // Get avg rating per user from Feedbacks
            var ratingData = await _context.Feedbacks
                .GroupBy(f => f.UserId)
                .Select(g => new { UserId = g.Key, AvgRating = g.Average(f => (double)f.Rating) })
                .ToListAsync();

            var ratingDict = ratingData.ToDictionary(r => r.UserId, r => r.AvgRating);

            var guests = bookingData.Select(g => new
            {
                g.UserId,
                FullName = g.FullName ?? "(Chưa cập nhật)",
                g.Phone,
                g.Email,
                g.Nationality,
                g.TotalSpent,
                g.TotalStays,
                g.TotalBookings,
                AvgRating     = ratingDict.TryGetValue(g.UserId, out var r) ? Math.Round(r, 1) : 0.0,
                g.LastStay
            }).ToList();

            // Sort
            var sorted = sort switch
            {
                "stays"  => guests.OrderByDescending(g => g.TotalStays),
                "rating" => guests.OrderByDescending(g => g.AvgRating),
                _        => guests.OrderByDescending(g => g.TotalSpent)
            };

            var result = sorted.Take(top).ToList<dynamic>();

            ViewBag.Sort = sort;
            ViewBag.Top  = top;
            return View(result);
        }
    }
}
