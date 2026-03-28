using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.NumberOfCustomers = await _context.Users.CountAsync(u => u.Role != null && u.Role.RoleName == "Customer");
            ViewBag.NumberOfBookings = await _context.Bookings.CountAsync();
            ViewBag.NumberOfRooms = await _context.Rooms.CountAsync(r => r.Status == "Available" || r.Status == "Tr?ng");
            ViewBag.NumberOfServices = await _context.Services.CountAsync();
            ViewBag.NumberOfFeedbacks = await _context.Feedbacks.CountAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BookingStatusList(BookingStatus? search, int page = 1)
        {
            const int pageSize = 5;
            var safePage = Math.Max(page, 1);

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .OrderByDescending(b => b.CreatedDate)
                .AsQueryable();

            if (search.HasValue)
            {
                var statusText = search.Value.ToString();
                query = query.Where(b => b.Status == statusText);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((safePage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;

            return View(new PagedResult<BookingViewModel>
            {
                Items = items.Select(MapBookingToViewModel).ToList(),
                TotalCount = totalCount,
                Page = safePage,
                PageSize = pageSize
            });
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetail(int id)
        {
            var booking = await LoadBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(MapBookingToViewModel(booking));
        }

        [HttpPost]
        public async Task<IActionResult> BookingDetail(int id, string? status)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                booking.Status = status.Trim();
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(BookingDetail), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> CheckIn(int id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.ActualCheckIn = DateTime.Now;
            booking.Status = BookingStatus.CheckedIn.ToString();
            await _context.SaveChangesAsync();

            TempData["Message"] = "Nhận phòng thành công!";
            return RedirectToAction(nameof(BookingStatusList));
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut(int id)
        {
            var booking = await LoadBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.ActualCheckOut = DateTime.Now;
            booking.Status = BookingStatus.CheckedOut.ToString();
            await _context.SaveChangesAsync();

            TempData["Message"] = "Trả phòng thành công!";
            return RedirectToAction(nameof(BookingStatusList));
        }

        private async Task<Booking?> LoadBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        private static BookingViewModel MapBookingToViewModel(Booking booking)
        {
            return new BookingViewModel
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                ExpectedCheckIn = booking.ExpectedCheckIn,
                ExpectedCheckOut = booking.ExpectedCheckOut,
                Deposit = booking.Deposit,
                NumOfPeople = booking.NumOfPeople,
                Status = booking.Status,
                CreatedDate = booking.CreatedDate,
                Room = booking.Room,
                Customer = booking.User,
                Services = booking.BookingServices
                    .Where(bs => bs.Service != null)
                    .Select(bs => bs.Service!)
                    .ToList()
            };
        }
    }
}
