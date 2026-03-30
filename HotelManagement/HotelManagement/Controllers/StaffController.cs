using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
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

        private readonly UserService _userService;
        private readonly BookingServiceHandle _bookingService;
        private readonly RoomService _roomService;
        private readonly ServiceHotelService _serviceHotel;
        private readonly InvoiceService _invoiceService;
        private readonly FeedbackService _feedbackService;
        private readonly ApplicationDbContext _context;

        public StaffController(
            UserService service,
            BookingServiceHandle bookingService,
            RoomService roomService,
            ServiceHotelService serviceHotel,
            InvoiceService invoiceService,
            FeedbackService feedbackService,
            ApplicationDbContext context)
        {
            _userService = service;
            _bookingService = bookingService;
            _roomService = roomService;
            _serviceHotel = serviceHotel;
            _invoiceService = invoiceService;
            _feedbackService = feedbackService;
            _context = context;
        }

        // ── Helper: Kiểm tra đăng nhập ──────────────────────────────
        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        // ── INDEX ────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            ViewBag.NumberOfCustomers = _userService.CountCustomer();
            ViewBag.NumberOfBookings = _bookingService.NumberOfBookings();
            ViewBag.NumberOfRooms = await _roomService.CountRooms();
            ViewBag.NumberOfServices = _serviceHotel.CountService();
            ViewBag.NumberOfFeedbacks = _feedbackService.CountFeedback();
            return View();
        }

        [HttpGet]
        public IActionResult Message()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            return View();
        }

        // ── CUSTOMER ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CustomerList(string? search, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var result = await _userService.GetCustomersAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerInfo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var customer = await _userService.GetCustomerById(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerHistory(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var history = await _bookingService.GetCustomerHistoryAsync(id);
            if (history == null) return NotFound();
            return View(history);
        }

        // ── ROOM ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> RoomList(string? search, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var result = await _roomService.GetAllRoomsAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        // ── BOOKING ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> BookingStatusList(BookingStatus? search, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var result = await _bookingService.GetAllBookings(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetail(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var services = await _serviceHotel.GetAllIncludingPenaltyAsync();
            ViewBag.Services = services;
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            var additionalCharges = await _context.AdditionalCharges
                .Where(x => x.BookingId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            ViewBag.AdditionalCharges = additionalCharges;
            ViewBag.AdditionalChargeTotal = additionalCharges.Sum(x => x.Amount);
            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> BookingDetail(int id, string? status)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            await _bookingService.BookingUpdateStatusAsync(id, status);
            TempData["Message"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction("BookingStatusList", "Staff");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServices(int bookingId, List<int>? selectedServiceIds)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            if (selectedServiceIds != null && selectedServiceIds.Any())
            {
                await _bookingService.AddServicesToBookingAsync(bookingId, selectedServiceIds);
                TempData["Message"] = "Cập nhật dịch vụ thành công!";
            }
            else
            {
                TempData["Warning"] = "Vui lòng chọn ít nhất một dịch vụ.";
            }

            return RedirectToAction("BookingDetail", "Staff", new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdditionalCharge(int bookingId, string? description, decimal? amount)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null) return NotFound();

            if (!string.Equals(booking.Status, "CheckedIn", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Warning"] = "Chi co the them phu phi khi booking dang o trang thai CheckedIn.";
                return RedirectToAction("BookingDetail", new { id = bookingId });
            }

            var normalizedDescription = description?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedDescription))
            {
                TempData["Warning"] = "Vui long nhap noi dung phu phi phat sinh.";
                return RedirectToAction("BookingDetail", new { id = bookingId });
            }

            if (amount == null || amount <= 0)
            {
                TempData["Warning"] = "So tien phu phi phai lon hon 0.";
                return RedirectToAction("BookingDetail", new { id = bookingId });
            }

            _context.AdditionalCharges.Add(new AdditionalCharge
            {
                BookingId = bookingId,
                Description = normalizedDescription,
                Amount = amount.Value,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = "Them phu phi phat sinh thanh cong!";
            return RedirectToAction("BookingDetail", new { id = bookingId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdditionalCharge(int bookingId, int additionalChargeId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var charge = await _context.AdditionalCharges
                .FirstOrDefaultAsync(x => x.AdditionalChargeId == additionalChargeId && x.BookingId == bookingId);

            if (charge == null)
            {
                TempData["Warning"] = "Khong tim thay phu phi can xoa.";
                return RedirectToAction("BookingDetail", new { id = bookingId });
            }

            _context.AdditionalCharges.Remove(charge);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Da xoa phu phi phat sinh.";
            return RedirectToAction("BookingDetail", new { id = bookingId });
        }

        [HttpGet]
        public async Task<IActionResult> CheckIn(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            await _bookingService.CheckInAsync(id);
            TempData["Message"] = "Nhận phòng thành công!";
            return RedirectToAction("BookingStatusList", "Staff");
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            int days = (booking.ExpectedCheckOut.Date - booking.ExpectedCheckIn.Date).Days;
            int numberOfDays = days > 0 ? days : 1;

            decimal roomPrice = (booking.Room?.Price ?? 0) * numberOfDays;
            decimal serviceTotal = booking.Services.Where(x => x.IsActive == true)?.Sum(s => s?.Price ?? 0) ?? 0;
            decimal surchargeTotal = booking.Services.Where(x => x.IsActive == false)?.Sum(s => s?.Price ?? 0) ?? 0;
            decimal deposit = booking.Deposit ?? 0;
            decimal baseAmount = GetBaseCheckoutAmount(roomPrice, serviceTotal, deposit);
            decimal totalAmount = baseAmount + additionalChargeTotal - deposit;

            ViewBag.NumberOfDays = numberOfDays;
            ViewBag.RoomPrice = roomPrice;
            ViewBag.ServiceTotal = serviceTotal;
            ViewBag.AdditionalChargeTotal = additionalChargeTotal;
            ViewBag.BaseCheckoutAmount = baseAmount;
            ViewBag.TotalToPay = totalAmount > 0 ? totalAmount : 0;
            ViewBag.SurchargeTotal = surchargeTotal;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckOut(int id, string paymentMethod)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            await _bookingService.CheckOutAsync(id, paymentMethod);

            var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.BookingId == id);
            if (invoice != null)
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                var additionalChargeTotal = await _context.AdditionalCharges
                    .Where(x => x.BookingId == id)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0;

                if (booking != null)
                {
                    int days = (booking.ExpectedCheckOut.Date - booking.ExpectedCheckIn.Date).Days;
                    int numberOfDays = days > 0 ? days : 1;
                    decimal roomPrice = (booking.Room?.Price ?? 0) * numberOfDays;
                    decimal serviceTotal = booking.Services?.Sum(s => s?.Price ?? 0) ?? 0;
                    decimal deposit = booking.Deposit ?? 0;
                    decimal baseAmount = GetBaseCheckoutAmount(roomPrice, serviceTotal, deposit);

                    invoice.TotalAmount = baseAmount + additionalChargeTotal;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Message"] = "Trả phòng và thanh toán thành công!";
            return RedirectToAction("BookingStatusList", "Staff");
        }

        // ── DIRECT BOOKING ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> BookingDirect(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var room = await _roomService.GetRoomById(id);
            if (room == null) return NotFound();

            var services = await _serviceHotel.GetAllAsync();
            ViewBag.Services = services;

            // Truncate to minutes – datetime-local input chỉ chấp nhận HH:mm
            var now = DateTime.Now;
            var nowTruncated = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            var model = new DirectBookingViewModel
            {
                RoomId        = room.RoomId,
                RoomNumber    = room.RoomNumber,
                RoomTypeName  = room.RoomTypeName,
                Price         = room.Price,
                CheckInDate   = nowTruncated,
                CheckOutDate  = nowTruncated.AddDays(1),
                NumberOfPeople = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingDirect(DirectBookingViewModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            // ── Date validation ──────────────────────────────────────
            if (model.CheckInDate.Date < DateTime.Now.Date)
                ModelState.AddModelError("CheckInDate", "Ngày check-in không được ở trong quá khứ.");

            if (model.CheckOutDate.Date <= model.CheckInDate.Date)
                ModelState.AddModelError("CheckOutDate", "Ngày check-out phải sau ngày check-in ít nhất 1 ngày.");

            if (!ModelState.IsValid)
            {
                var services = await _serviceHotel.GetAllAsync();
                ViewBag.Services = services;
                return View(model);
            }

            await _bookingService.CreateBookingDirectAsync(model);
            TempData["Message"] = "Đặt phòng trực tiếp thành công!";
            return RedirectToAction("BookingStatusList", "Staff");
        }

        // ── INVOICE ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> InvoiceList(string? search, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var invoices = await _invoiceService.GetAllInvoicesAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> InvoiceDetail(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // ── FEEDBACK ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ViewFeedback(string? search, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            int pageSize = 5;
            var result = await _feedbackService.GetAllFeedback(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> FeedbackDetail(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var result = await _feedbackService.GetFeedbackById(id);
            if (result == null) return NotFound();
            return View(result);
        }
    }
}

