using HotelManagement.Context;
using HotelManagement.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ── Revenue by Room Type ─────────────────────────────────
            var revenueByRoomType = await _context.Invoices
                .Where(i => i.Status == "Paid")
                .Include(i => i.Booking).ThenInclude(b => b.Room)
                .GroupBy(i => i.Booking.Room.RoomTypeName)
                .Select(g => new
                {
                    RoomType = g.Key ?? "Không rõ",
                    Revenue  = g.Sum(i => i.TotalAmount),
                    Count    = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            // ── Most popular services (by booking count) ─────────────
            var servicePopularity = await _context.BookingServices
                .Where(bs => bs.Service != null)
                .GroupBy(bs => bs.Service!.Name)
                .Select(g => new
                {
                    ServiceName = g.Key,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(bs => (decimal?)bs.Service!.Price) ?? 0m
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(10)
                .ToListAsync();

            // ── Average rating by Room ───────────────────────────────
            var ratingByRoom = await _context.Feedbacks
                .Include(f => f.Room)
                .GroupBy(f => f.Room.RoomNumber)
                .Select(g => new
                {
                    RoomNumber    = g.Key ?? "Không rõ",
                    AvgRating     = g.Average(f => (double)f.Rating),
                    FeedbackCount = g.Count()
                })
                .OrderByDescending(x => x.AvgRating)
                .Take(10)
                .ToListAsync();

            // ── Average Length of Stay (ALOS) ────────────────────────
            var checkedOutBookings = await _context.Bookings
                .Where(b => b.Status == "CheckedOut"
                         && b.ActualCheckIn.HasValue
                         && b.ActualCheckOut.HasValue)
                .Select(b => new { CheckIn = b.ActualCheckIn!.Value, CheckOut = b.ActualCheckOut!.Value })
                .ToListAsync();

            double alos = checkedOutBookings.Count > 0
                ? Math.Round(checkedOutBookings.Average(x => (x.CheckOut - x.CheckIn).TotalDays), 1)
                : 0.0;
            int totalCheckedOut = checkedOutBookings.Count;

            // ── Most booked rooms ────────────────────────────────────
            var mostBookedRooms = await _context.Bookings
                .Include(b => b.Room)
                .GroupBy(b => new { b.RoomId, b.Room.RoomNumber, b.Room.RoomTypeName })
                .Select(g => new
                {
                    RoomNumber   = g.Key.RoomNumber,
                    RoomTypeName = g.Key.RoomTypeName,
                    BookingCount = g.Count(),
                    Revenue      = g.Where(b => b.Invoice != null && b.Invoice.Status == "Paid")
                                    .Sum(b => (decimal?)b.Invoice!.TotalAmount) ?? 0m
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(8)
                .ToListAsync();

            // ── Monthly revenue (last 12 months) ─────────────────────
            var now = DateTime.Today;
            var twelveMonthsAgo = now.AddMonths(-11);
            var monthlyRevenue = await _context.Invoices
                .Where(i => i.Status == "Paid"
                         && i.PaymentDate.HasValue
                         && i.PaymentDate.Value >= new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1))
                .GroupBy(i => new { i.PaymentDate!.Value.Year, i.PaymentDate!.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.TotalAmount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var monthLabels = new List<string>();
            var monthValues = new List<decimal>();
            for (int m = 0; m < 12; m++)
            {
                var d = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1).AddMonths(m);
                monthLabels.Add(d.ToString("MM/yyyy"));
                var match = monthlyRevenue.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
                monthValues.Add(match?.Total ?? 0m);
            }

            // Serialize for charts
            ViewBag.RoomTypeLabels  = System.Text.Json.JsonSerializer.Serialize(revenueByRoomType.Select(x => x.RoomType).ToArray());
            ViewBag.RoomTypeRevenue = System.Text.Json.JsonSerializer.Serialize(revenueByRoomType.Select(x => x.Revenue).ToArray());
            ViewBag.RoomTypeCount   = System.Text.Json.JsonSerializer.Serialize(revenueByRoomType.Select(x => x.Count).ToArray());

            ViewBag.ServiceLabels   = System.Text.Json.JsonSerializer.Serialize(servicePopularity.Select(x => x.ServiceName).ToArray());
            ViewBag.ServiceCounts   = System.Text.Json.JsonSerializer.Serialize(servicePopularity.Select(x => x.BookingCount).ToArray());

            ViewBag.RoomRatingLabels = System.Text.Json.JsonSerializer.Serialize(ratingByRoom.Select(x => "Phòng " + x.RoomNumber).ToArray());
            ViewBag.RoomRatingValues = System.Text.Json.JsonSerializer.Serialize(ratingByRoom.Select(x => Math.Round(x.AvgRating, 1)).ToArray());
            ViewBag.RoomFeedbackCounts = System.Text.Json.JsonSerializer.Serialize(ratingByRoom.Select(x => x.FeedbackCount).ToArray());

            ViewBag.MonthLabels  = System.Text.Json.JsonSerializer.Serialize(monthLabels);
            ViewBag.MonthValues  = System.Text.Json.JsonSerializer.Serialize(monthValues);

            ViewBag.Alos             = alos;
            ViewBag.TotalCheckedOut  = totalCheckedOut;
            ViewBag.MostBookedRooms  = mostBookedRooms.Cast<dynamic>().ToList();
            ViewBag.ServicePopularity = servicePopularity.Cast<dynamic>().ToList();
            ViewBag.RevenueByRoomType = revenueByRoomType.Cast<dynamic>().ToList();

            return View();
        }
    }
}
