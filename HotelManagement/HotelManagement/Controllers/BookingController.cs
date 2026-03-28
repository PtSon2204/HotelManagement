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
        private readonly RoomService _roomService;
        private readonly ApplicationDbContext _context;

        public BookingController(RoomService roomService, ApplicationDbContext context)
        {
            _roomService = roomService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? roomId, int? roomTypeId, string? checkIn, string? checkOut, int? adults, int? children, int? rooms)
        {
            var model = new DirectBookingViewModel
            {
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1)
            };

            if (roomId.HasValue)
            {
                model.RoomId = roomId.Value;
                ViewBag.IsRoomLocked = true;

                var roomEntity = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId.Value);
                if (roomEntity != null)
                {
                    model.RoomNumber = roomEntity.RoomNumber;
                    model.RoomTypeName = roomEntity.RoomTypeName;
                    model.Price = roomEntity.Price;
                    model.NumberOfPeople = roomEntity.Capacity;
                    ViewBag.FixedCapacity = roomEntity.Capacity;
                }
            }

            if (!string.IsNullOrEmpty(checkIn))
            {
                if (DateTime.TryParse(checkIn, out var checkInDate))
                {
                    model.CheckInDate = checkInDate;
                }
            }

            if (!string.IsNullOrEmpty(checkOut))
            {
                if (DateTime.TryParse(checkOut, out var checkOutDate))
                {
                    model.CheckOutDate = checkOutDate;
                }
            }

            if (ViewBag.FixedCapacity == null)
            {
                var totalPeople = (adults ?? 1) + (children ?? 0);
                model.NumberOfPeople = totalPeople > 0 ? totalPeople : 1;
            }

            await FillCurrentUserInfoAsync(model);
            await RepopulateCreateViewBag(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DirectBookingViewModel model)
        {
            if (model.CheckInDate < DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.CheckInDate), "Ngày nhận phòng không thể ở trong quá khứ.");
            }

            if (model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError(nameof(model.CheckOutDate), "Ngày trả phòng phải sau ngày nhận phòng.");
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == model.RoomId);
            if (room == null)
            {
                ModelState.AddModelError(nameof(model.RoomId), "Không tìm thấy phòng bạn đã chọn.");
            }

            if (!ModelState.IsValid)
            {
                await RepopulateCreateViewBag(model);
                return View(model);
            }

            var hasConflict = await _context.Bookings.AnyAsync(b =>
                b.RoomId == model.RoomId &&
                b.Status != BookingStatus.Cancelled.ToString() &&
                model.CheckInDate < b.ExpectedCheckOut &&
                model.CheckOutDate > b.ExpectedCheckIn);

            if (hasConflict)
            {
                ModelState.AddModelError(nameof(model.RoomId), "Phòng này đã được đặt trong khoảng thời gian bạn chọn.");
                await RepopulateCreateViewBag(model);
                return View(model);
            }

            var username = HttpContext.Session.GetString("Username");
            User? user = null;
            if (!string.IsNullOrWhiteSpace(username))
            {
                user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == username);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Bạn cần đăng nhập để đặt phòng.");
                await RepopulateCreateViewBag(model);
                return View(model);
            }

            var booking = new Booking
            {
                UserId = user.UserId,
                RoomId = model.RoomId,
                ExpectedCheckIn = model.CheckInDate,
                ExpectedCheckOut = model.CheckOutDate,
                Deposit = 0,
                NumOfPeople = model.NumberOfPeople,
                Status = BookingStatus.Pending.ToString(),
                CreatedDate = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            if (model.SelectedServiceIds.Count > 0)
            {
                var bookingServices = model.SelectedServiceIds
                    .Distinct()
                    .Select(serviceId => new BookingService
                    {
                        BookingId = booking.BookingId,
                        ServiceId = serviceId
                    })
                    .ToList();

                _context.BookingServices.AddRange(bookingServices);
                await _context.SaveChangesAsync();
            }

            TempData["BookingSuccess"] = "Đặt phòng thành công.";
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrWhiteSpace(username))
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử đặt phòng.";
                return RedirectToAction("LoginRegister", "Account");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingServices)
                    .ThenInclude(bs => bs.Service)
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }

        private async Task FillCurrentUserInfoAsync(DirectBookingViewModel model)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return;
            }

            model.UserId = user.UserId;
            model.FullName = user.FullName ?? model.FullName;
            model.Phone = user.Phone ?? model.Phone;
            model.Email = user.Email;
            model.Address = user.Address;
            model.IdCard = user.IDCard;
            model.Nationality = user.Nationality;
            model.Gender = user.Gender;

            if (user.Role?.RoleName == "Staff")
            {
                model.StaffId = user.UserId;
            }
        }

        private async Task RepopulateCreateViewBag(DirectBookingViewModel model)
        {
            var rooms = await _roomService.GetAllAsync();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomNumber", model.RoomId);
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.Name)
                .ToListAsync();

            if (model.RoomId > 0)
            {
                ViewBag.IsRoomLocked = true;
                var roomEntity = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == model.RoomId);
                if (roomEntity != null)
                {
                    ViewBag.FixedCapacity = roomEntity.Capacity;
                }
            }
        }
    }
}
