using HotelManagement.Context;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingServiceHanlde _bookingService;
        private readonly RoomService _roomService;
        private readonly HotelServiceService _hotelServiceService;
        private readonly ApplicationDbContext _context;

        public BookingController(
            BookingServiceHanlde bookingService, 
            RoomService roomService, 
            HotelServiceService hotelServiceService,
            ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _hotelServiceService = hotelServiceService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? roomId, int? roomTypeId, string? checkIn, string? checkOut, int? adults, int? children, int? rooms)
        {
            var model = new DirectBookingViewModel();
            model.CheckInDate = DateTime.Now;
            model.CheckOutDate = DateTime.Now.AddDays(1);

            if (roomId.HasValue)
            {
                model.RoomId = roomId.Value;
                ViewBag.IsRoomLocked = true;
            }
            
            if (!string.IsNullOrEmpty(checkIn))
            {
                if (DateTime.TryParseExact(checkIn, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d1))
                    model.CheckInDate = d1;
                else if (DateTime.TryParse(checkIn, out var d1Fallback))
                    model.CheckInDate = d1Fallback;
            }

            if (!string.IsNullOrEmpty(checkOut))
            {
                if (DateTime.TryParseExact(checkOut, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2))
                    model.CheckOutDate = d2;
                else if (DateTime.TryParse(checkOut, out var d2Fallback))
                    model.CheckOutDate = d2Fallback;
            }

            int totalPeople = (adults ?? 1) + (children ?? 0);
            model.NumberOfPeople = totalPeople > 0 ? totalPeople : 1;

            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users
                    .Include(u => u.Customer)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user?.Customer != null)
                {
                    model.FullName = user.Customer.FullName;
                    model.Phone = user.Customer.Phone ?? "";
                    model.Email = user.Customer.Email;
                    model.Address = user.Customer.Address;
                    model.IdCard = user.Customer.Idcard;
                    model.Nationality = user.Customer.Nationality;
                    model.Gender = user.Customer.Gender;
                }
                
                if (user != null && user.Role == "Staff" && user.StaffId != null)
                {
                    model.StaffId = user.StaffId;
                }
            }

            var roomList = await _roomService.GetAllAsync();
            if (roomTypeId.HasValue)
            {
                roomList = roomList.Where(r => r.RoomTypeId == roomTypeId.Value).ToList();
            }
            ViewBag.Rooms = new SelectList(roomList, "RoomId", "RoomNumber");

            var services = await _hotelServiceService.GetAllAsync();
            ViewBag.Services = services.Where(s => s.IsActive == true).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DirectBookingViewModel model)
        {
            if (model.CheckInDate.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không thể ở trong quá khứ.");
            }

            if (model.CheckOutDate.Date <= model.CheckInDate.Date)
            {
                ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải sau ngày nhận phòng ít nhất 1 ngày.");
            }

            if (!ModelState.IsValid)
            {
                var rooms = await _roomService.GetAllAsync();
                ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);
                var services = await _hotelServiceService.GetAllAsync();
                ViewBag.Services = services.Where(s => s.IsActive == true).ToList();
                return View(model);
            }

            bool isAvailable = await _bookingService.IsRoomAvailableAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);
            if (!isAvailable)
            {
                ModelState.AddModelError("RoomId", "Phòng này đã được đặt hoặc đang sử dụng trong khoảng thời gian bạn chọn. Vui lòng thử thời gian khác!");
                var rooms = await _roomService.GetAllAsync();
                ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);
                var services = await _hotelServiceService.GetAllAsync();
                ViewBag.Services = services.Where(s => s.IsActive == true).ToList();
                return View(model);
            }

            try
            {
                var username = HttpContext.Session.GetString("Username");
                if (!string.IsNullOrEmpty(username))
                {
                     var user = await _context.Users.Include(u => u.Customer).FirstOrDefaultAsync(u => u.Username == username);
                     if (user != null)
                     {
                         if (user.Role == "Staff" && user.StaffId != null && model.StaffId == null)
                         {
                             model.StaffId = user.StaffId;
                         }
                         if (user.Role != "Staff" && user.Customer != null)
                         {
                             model.CustomerId = user.Customer.CustomerId;
                         }
                     }
                }

                int bookingId = await _bookingService.CreateBookingDirectAsync(model);
                TempData["BookingSuccess"] = "Đặt phòng thành công! Xin cảm ơn quý khách.";
                return RedirectToAction("Payment", "Invoice", new { bookingId = bookingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt phòng: " + ex.Message);
                var rooms = await _roomService.GetAllAsync();
                ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);
                var services = await _hotelServiceService.GetAllAsync();
                ViewBag.Services = services.Where(s => s.IsActive == true).ToList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user?.Customer == null)
            {
                TempData["Error"] = "Vui lòng cập nhật thông tin cá nhân (Profile) trước khi xem lịch sử đặt phòng.";
                return RedirectToAction("Profile", "Account");
            }

            var bookings = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room).ThenInclude(r => r.RoomType)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .Where(b => b.CustomerId == user.Customer.CustomerId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }
    }
}
