using HotelManagement.Context;
using HotelManagement.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var thirtyDaysAgo = today.AddDays(-29);

            // ── KPI Cards ──────────────────────────────────────────────
            var todayRevenue = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.PaymentDate.HasValue
                         && i.PaymentDate.Value.Date == today)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            var totalRooms = await _context.Rooms.CountAsync(r => r.IsActive);
            var occupiedRooms = await _context.Rooms
                .CountAsync(r => r.IsActive && r.Status == "Occupied");
            var occupancyRate = totalRooms > 0
                ? Math.Round((double)occupiedRooms / totalRooms * 100, 1)
                : 0.0;

            var pendingCheckInsToday = await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed"
                              && b.ExpectedCheckIn.Date == today);

            var totalFeedbacks = await _context.Feedbacks.CountAsync();
            var avgRating = await _context.Feedbacks.AnyAsync()
                ? await _context.Feedbacks.AverageAsync(f => (double)f.Rating)
                : 0.0;

            var monthRevenue = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.PaymentDate.HasValue
                         && i.PaymentDate.Value.Month == today.Month
                         && i.PaymentDate.Value.Year == today.Year)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            var pendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Pending");

            // ── Revenue trend: last 30 days ────────────────────────────
            var revenueTrend = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.PaymentDate.HasValue
                         && i.PaymentDate.Value.Date >= thirtyDaysAgo
                         && i.PaymentDate.Value.Date <= today)
                .GroupBy(i => i.PaymentDate!.Value.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(i => i.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Fill in missing days with 0
            var trendLabels = new List<string>();
            var trendValues = new List<decimal>();
            for (int d = 0; d < 30; d++)
            {
                var date = thirtyDaysAgo.AddDays(d);
                trendLabels.Add(date.ToString("dd/MM"));
                var match = revenueTrend.FirstOrDefault(x => x.Date == date);
                trendValues.Add(match?.Total ?? 0m);
            }

            // ── Revenue breakdown (pie) ────────────────────────────────
            // Room revenue = sum of (RoomPrice * nights) per paid invoice
            var paidInvoices = await _context.Invoices
                .Where(i => i.Status == "Paid")
                .Include(i => i.Booking).ThenInclude(b => b.Room)
                .Include(i => i.Surcharges)
                .Include(i => i.Booking).ThenInclude(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .ToListAsync();

            decimal roomRevenue = 0, serviceRevenue = 0, surchargeRevenue = 0;
            foreach (var inv in paidInvoices)
            {
                var nights = Math.Max(1,
                    (inv.Booking.ExpectedCheckOut.Date - inv.Booking.ExpectedCheckIn.Date).Days);
                roomRevenue += (inv.Booking.Room?.Price ?? 0) * nights;
                serviceRevenue += inv.Booking.BookingServices
                    .Where(bs => bs.Service != null)
                    .Sum(bs => bs.Service!.Price);
                surchargeRevenue += inv.Surcharges.Sum(s => s.Amount);
            }

            // ── Booking status breakdown (bar) ─────────────────────────
            var bookingStatuses = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.MonthRevenue = monthRevenue;
            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.OccupiedRooms = occupiedRooms;
            ViewBag.TotalRooms = totalRooms;
            ViewBag.PendingCheckInsToday = pendingCheckInsToday;
            ViewBag.PendingBookings = pendingBookings;
            ViewBag.TotalFeedbacks = totalFeedbacks;
            ViewBag.AvgRating = Math.Round(avgRating, 1);

            ViewBag.TrendLabels = System.Text.Json.JsonSerializer.Serialize(trendLabels);
            ViewBag.TrendValues = System.Text.Json.JsonSerializer.Serialize(trendValues);

            ViewBag.RoomRevenue = roomRevenue;
            ViewBag.ServiceRevenue = serviceRevenue;
            ViewBag.SurchargeRevenue = surchargeRevenue;

            var statusOrder = new[] { "Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
            ViewBag.BookingStatusLabels = System.Text.Json.JsonSerializer.Serialize(statusOrder);
            ViewBag.BookingStatusValues = System.Text.Json.JsonSerializer.Serialize(
                statusOrder.Select(s => bookingStatuses.FirstOrDefault(x => x.Status == s)?.Count ?? 0).ToArray()
            );

            return View();
        }
    }
}
