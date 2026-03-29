using HotelManagement.Context;
using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingServiceHandle _bookingService;
        private readonly ApplicationDbContext _context;

        public BookingController(BookingServiceHandle bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // GET: /Booking/Create?roomId=X&checkIn=...&checkOut=...
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create(int? roomId, string? checkIn, string? checkOut)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LoginRegister", "Account");

            var model = new DirectBookingViewModel
            {
                CheckInDate = DateTime.Now.Date.AddDays(1),
                CheckOutDate = DateTime.Now.Date.AddDays(2)
            };

            // Điền ngày từ query string (hỗ trợ cả dd/MM/yyyy lẫn yyyy-MM-dd)
            model.CheckInDate = ParseDate(checkIn) ?? model.CheckInDate;
            model.CheckOutDate = ParseDate(checkOut) ?? model.CheckOutDate;

            // Nạp thông tin người dùng hiện tại
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                // Mặc định ban đầu điền thông tin khách = chính chủ tài khoản
                model.UserId       = user.UserId;
                model.FullName     = user.FullName ?? "";
                model.Phone        = user.Phone ?? "";
                model.Email        = user.Email ?? "";
                model.Address      = user.Address ?? "";
                model.IdCard       = user.IDCard ?? "";
                model.Nationality  = user.Nationality ?? "";
                model.Gender       = user.Gender ?? "";
                
                model.AccountName  = user.FullName ?? user.Username;
                model.AccountPhone = user.Phone;
            }

            // Khoá phòng nếu roomId được truyền vào
            if (roomId.HasValue)
            {
                model.RoomId = roomId.Value;
                ViewBag.IsRoomLocked = true;

                var roomEntity = await _context.Rooms
                    .Include(r => r.Images)
                    .FirstOrDefaultAsync(r => r.RoomId == roomId.Value);
                if (roomEntity != null)
                {
                    model.NumberOfPeople  = roomEntity.Capacity;
                    model.Price           = roomEntity.Price;   // dùng cho JS price calculator
                    ViewBag.FixedCapacity = roomEntity.Capacity;
                    ViewBag.Room          = roomEntity;         // dùng để hiển thị sidebar & basePrice
                }
            }

            await PopulateViewBagAsync(model, user?.UserId);
            return View(model);
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // POST: /Booking/Create
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DirectBookingViewModel model)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LoginRegister", "Account");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return RedirectToAction("LoginRegister", "Account");

            model.UserId = user.UserId;

            // ──── Business Validation ────
            if (model.CheckInDate.Date < DateTime.Now.Date)
                ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không được ở trong quá khứ.");

            if (model.CheckOutDate.Date <= model.CheckInDate.Date)
                ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải sau ngày nhận phòng ít nhất 1 ngày.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Lưu ý: " + string.Join(" | ", errors);
                model.AccountName = user.FullName ?? user.Username;
                model.AccountPhone = user.Phone;
                await PopulateViewBagAsync(model, user.UserId);
                return View(model);
            }

            bool available = await _bookingService.IsRoomAvailableAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);
            if (!available)
            {
                ModelState.AddModelError("RoomId", "Phòng đã được đặt trong khoảng thời gian này. Vui lòng chọn ngày khác hoặc phòng khác.");
                TempData["Error"] = "Phòng đã được đặt trong khoảng thời gian này. Vui lòng chọn ngày khác.";
                model.AccountName = user.FullName ?? user.Username;
                model.AccountPhone = user.Phone;
                await PopulateViewBagAsync(model, user.UserId);
                return View(model);
            }

            try
            {
                int bookingId = await _bookingService.CreateBookingAsync(model, user.UserId);
                TempData["BookingSuccess"] = "Đặt phòng thành công! Vui lòng hoàn tất thanh toán.";
                return RedirectToAction("Payment", "Invoice", new { bookingId });
            }
            catch (Exception ex)
            {
                string errMsg = "Có lỗi xảy ra: " + ex.Message;
                if (ex.InnerException != null) errMsg += " - " + ex.InnerException.Message;
                
                ModelState.AddModelError("", errMsg);
                TempData["Error"] = errMsg;
                model.AccountName = user.FullName ?? user.Username;
                model.AccountPhone = user.Phone;
                await PopulateViewBagAsync(model, user.UserId);
                return View(model);
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // GET: /Booking/MyBookings
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LoginRegister", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("LoginRegister", "Account");

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.GuestProfile)
                .Include(b => b.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .Include(b => b.Invoice)
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────────────────────────
        private async Task PopulateViewBagAsync(DirectBookingViewModel model, int? userId = null)
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsActive && r.Status == "Available")
                .ToListAsync();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);

            var services = await _context.Services
                .Where(s => s.IsActive == true)
                .ToListAsync();
            ViewBag.Services = services;

            if (model.RoomId > 0)
            {
                ViewBag.IsRoomLocked = true;
                var room = await _context.Rooms
                    .Include(r => r.Images)
                    .FirstOrDefaultAsync(r => r.RoomId == model.RoomId);
                if (room != null)
                {
                    ViewBag.FixedCapacity = room.Capacity;
                    ViewBag.Room          = room;   // cần để sidebar & basePrice hoạt động
                }
            }
            
            if (userId.HasValue)
            {
                var savedProfiles = await _context.GuestProfiles
                    .Where(p => p.UserId == userId.Value)
                    .OrderByDescending(p => p.ProfileId)
                    .Select(p => new {
                        p.ProfileId,
                        p.Label,
                        p.FullName,
                        p.Phone,
                        p.Email,
                        p.IdCard,
                        p.Gender,
                        p.Nationality,
                        p.Address
                    })
                    .ToListAsync();
                ViewBag.SavedProfiles = savedProfiles;
            }
        }

        private static DateTime? ParseDate(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParseExact(raw, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d1)) return d1;
            if (DateTime.TryParse(raw, out var d2)) return d2;
            return null;
        }

        public IActionResult PrivacyHotel()
        {
            return View();
        }
    }
}
