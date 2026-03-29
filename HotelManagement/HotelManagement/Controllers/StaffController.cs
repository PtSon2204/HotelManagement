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

        public StaffController(
            UserService service,
            BookingServiceHandle bookingService,
            RoomService roomService,
            ServiceHotelService serviceHotel,
            InvoiceService invoiceService,
            FeedbackService feedbackService)
        {
            _userService = service;
            _bookingService = bookingService;
            _roomService = roomService;
            _serviceHotel = serviceHotel;
            _invoiceService = invoiceService;
            _feedbackService = feedbackService;
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

            var services = await _serviceHotel.GetAllAsync();
            ViewBag.Services = services;
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();
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
            decimal serviceTotal = booking.Services?.Sum(s => s?.Price ?? 0) ?? 0;
            decimal deposit = booking.Deposit ?? 0;
            decimal totalAmount = roomPrice + serviceTotal - deposit;

            ViewBag.NumberOfDays = numberOfDays;
            ViewBag.RoomPrice = roomPrice;
            ViewBag.ServiceTotal = serviceTotal;
            ViewBag.TotalToPay = totalAmount > 0 ? totalAmount : 0;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckOut(int id, string paymentMethod)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            await _bookingService.CheckOutAsync(id, paymentMethod);
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

