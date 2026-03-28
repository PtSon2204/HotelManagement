//using HotelManagement.Context;
//using HotelManagement.Models.Entities;
//using HotelManagement.Models.ViewModels;
//using HotelManagement.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//namespace HotelManagement.Controllers
//{
//    public class BookingController : Controller
//    {
//        private readonly BookingServiceHanlde _bookingService;
//        private readonly RoomService _roomService;
//        private readonly HotelServiceService _hotelServiceService;
//        private readonly ApplicationDbContext _context;

//        public BookingController(
//            BookingServiceHanlde bookingService,
//            RoomService roomService,
//            HotelServiceService hotelServiceService,
//            ApplicationDbContext context)
//        {
//            _bookingService = bookingService;
//            _roomService = roomService;
//            _hotelServiceService = hotelServiceService;
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Create(int? roomId, int? roomTypeId, string? checkIn, string? checkOut, int? adults, int? children, int? rooms)
//        {
//            var model = new DirectBookingViewModel
//            {
//                CheckInDate = DateTime.Now,
//                CheckOutDate = DateTime.Now.AddDays(1)
//            };

//            if (roomId.HasValue)
//            {
//                model.RoomId = roomId.Value;
//                ViewBag.IsRoomLocked = true;

//                var roomEntity = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId.Value);
//                if (roomEntity != null)
//                {
//                    model.NumberOfPeople = roomEntity.Capacity;
//                    ViewBag.FixedCapacity = roomEntity.Capacity;
//                }
//            }

//            if (!string.IsNullOrEmpty(checkIn))
//            {
//                if (DateTime.TryParseExact(checkIn, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d1))
//                    model.CheckInDate = d1;
//                else if (DateTime.TryParse(checkIn, out var d1Fallback))
//                    model.CheckInDate = d1Fallback;
//            }

//            if (!string.IsNullOrEmpty(checkOut))
//            {
//                if (DateTime.TryParseExact(checkOut, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d2))
//                    model.CheckOutDate = d2;
//                else if (DateTime.TryParse(checkOut, out var d2Fallback))
//                    model.CheckOutDate = d2Fallback;
//            }

//            if (ViewBag.FixedCapacity == null)
//            {
//                int totalPeople = (adults ?? 1) + (children ?? 0);
//                model.NumberOfPeople = totalPeople > 0 ? totalPeople : 1;
//            }

//            // Lấy thông tin user đang đăng nhập
//            var username = HttpContext.Session.GetString("Username");
//            if (!string.IsNullOrEmpty(username))
//            {
//                var user = await _context.Users
//                    .Include(u => u.Role)
//                    .FirstOrDefaultAsync(u => u.Username == username);

//                if (user != null)
//                {
//                    // Điền sẵn thông tin từ User
//                    model.FullName = user.FullName;
//                    model.Phone = user.Phone ?? "";
//                    model.Email = user.Email;
//                    model.Address = user.Address;
//                    model.IdCard = user.IDCard;
//                    model.Nationality = user.Nationality;
//                    model.Gender = user.Gender;

//                    // Nếu là Staff thì lưu StaffId
//                    if (user.Role?.RoleName == "Staff")
//                    {
//                        model.StaffId = user.UserId;
//                    }
//                    else
//                    {
//                        model.UserId = user.UserId;
//                    }
//                }
//            }

//            var roomList = await _roomService.GetAllAsync();
//            ViewBag.Rooms = new SelectList(roomList, "RoomId", "RoomNumber");

//            var services = await _hotelServiceService.GetAllAsync();
//            ViewBag.Services = services.Where(s => s.IsActive == true).ToList();

//            return View(model);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(DirectBookingViewModel model)
//        {
//            if (model.CheckInDate.Date < DateTime.Now.Date)
//                ModelState.AddModelError("CheckInDate", "Ngày nhận phòng không thể ở trong quá khứ.");

//            if (model.CheckOutDate.Date <= model.CheckInDate.Date)
//                ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải sau ngày nhận phòng ít nhất 1 ngày.");

//            if (!ModelState.IsValid)
//            {
//                await RepopulateCreateViewBag(model);
//                return View(model);
//            }

//            bool isAvailable = await _bookingService.IsRoomAvailableAsync(model.RoomId, model.CheckInDate, model.CheckOutDate);
//            if (!isAvailable)
//            {
//                ModelState.AddModelError("RoomId", "Phòng này đã được đặt hoặc đang sử dụng trong khoảng thời gian bạn chọn. Vui lòng thử thời gian khác!");
//                await RepopulateCreateViewBag(model);
//                return View(model);
//            }

//            try
//            {
//                var username = HttpContext.Session.GetString("Username");
//                if (!string.IsNullOrEmpty(username))
//                {
//                    var user = await _context.Users
//                        .Include(u => u.Role)
//                        .FirstOrDefaultAsync(u => u.Username == username);

//                    if (user != null)
//                    {
//                        if (user.Role?.RoleName == "Staff" && model.StaffId == null)
//                            model.StaffId = user.UserId;

//                        if (user.Role?.RoleName != "Staff")
//                            model.UserId = user.UserId;
//                    }
//                }

//                int bookingId = await _bookingService.CreateBookingDirectAsync(model);
//                return RedirectToAction("Payment", "Invoice", new { bookingId = bookingId });
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt phòng: " + ex.Message);
//                await RepopulateCreateViewBag(model);
//                return View(model);
//            }
//        }

//        private async Task RepopulateCreateViewBag(DirectBookingViewModel model)
//        {
//            var rooms = await _roomService.GetAllAsync();
//            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);
//            var services = await _hotelServiceService.GetAllAsync();
//            ViewBag.Services = services.Where(s => s.IsActive == true).ToList();

//            if (model.RoomId > 0)
//            {
//                ViewBag.IsRoomLocked = true;
//                var roomEntity = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == model.RoomId);
//                if (roomEntity != null)
//                    ViewBag.FixedCapacity = roomEntity.Capacity;
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> MyBookings()
//        {
//            var username = HttpContext.Session.GetString("Username");
//            if (string.IsNullOrEmpty(username))
//                return RedirectToAction("Login", "Account");

//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

//            if (user == null)
//            {
//                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử đặt phòng.";
//                return RedirectToAction("Profile", "Account");
//            }

//            var bookings = await _context.Bookings
//                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
//                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
//                .Where(b => b.UserId == user.UserId)
//                .OrderByDescending(b => b.CreatedDate)
//                .ToListAsync();

//            return View(bookings);
//        }
//    }
//}
