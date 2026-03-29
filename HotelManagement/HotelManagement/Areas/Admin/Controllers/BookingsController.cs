using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class BookingsController : Controller
    {
        private readonly BookingRepository _bookingRepository;
        private readonly ApplicationDbContext _context;

        public BookingsController(BookingRepository bookingRepository, ApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(BookingStatus? status, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value.ToString());

            if (fromDate.HasValue)
                query = query.Where(b => b.ExpectedCheckIn.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(b => b.ExpectedCheckIn.Date <= toDate.Value.Date);

            int total = await query.CountAsync();
            var bookings = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = bookings.Select(b => new Models.ViewModels.BookingViewModel
            {
                BookingId        = b.BookingId,
                Status           = b.Status,
                ExpectedCheckIn  = b.ExpectedCheckIn,
                ExpectedCheckOut = b.ExpectedCheckOut,
                ActualCheckIn    = b.ActualCheckIn,
                ActualCheckOut   = b.ActualCheckOut,
                NumOfPeople      = b.NumOfPeople,
                Deposit          = b.Deposit,
                CreatedDate      = b.CreatedDate,
                Room             = b.Room,
                Customer         = b.User,
                Services         = b.BookingServices
                    .Where(bs => bs.Service != null)
                    .Select(bs => bs.Service!).ToList()
            }).ToList();

            var result = new Models.Common.PagedResult<Models.ViewModels.BookingViewModel>
            {
                Items     = items,
                TotalCount = total,
                Page      = page,
                PageSize  = pageSize
            };

            ViewBag.CurrentStatus = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate   = toDate?.ToString("yyyy-MM-dd");
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingRepository.GetBookingById(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            await _bookingRepository.CheckIn(id);
            TempData["Success"] = "Nhận phòng thành công!";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id, string paymentMethod = "Tiền mặt")
        {
            await _bookingRepository.CheckOut(id, paymentMethod);
            TempData["Success"] = "Trả phòng và thanh toán thành công!";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            await _bookingRepository.UpdateStatus(id, BookingStatus.Cancelled.ToString());
            TempData["Success"] = "Hủy đặt phòng thành công!";
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
