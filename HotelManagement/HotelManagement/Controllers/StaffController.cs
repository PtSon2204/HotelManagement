using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class StaffController : Controller
    {
        private readonly CustomerService _customerService;
        private readonly BookingServiceHanlde _bookingService;
        private readonly RoomService _roomService;
        private readonly HotelServiceService _serviceHotel;
        private readonly InvoiceService _invoiceService;
        private readonly FeedbackService _feedbackService;

        public StaffController(
            CustomerService service,
            BookingServiceHanlde bookingService,
            RoomService roomService,
            HotelServiceService serviceHotel,
            InvoiceService invoiceService,
            FeedbackService feedbackService)
        {
            _customerService = service;
            _bookingService = bookingService;
            _roomService = roomService;
            _serviceHotel = serviceHotel;
            _invoiceService = invoiceService;
            _feedbackService = feedbackService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.NumberOfCustomers = _customerService.CountCustomer();
            ViewBag.NumberOfBookings = _bookingService.NumberOfBookings();
            ViewBag.NumberOfRooms = await _roomService.CountRooms();
            ViewBag.NumberOfServices = _serviceHotel.CountService();
            ViewBag.NumberOfFeedbacks = _feedbackService.CountFeedback();
            return View();
        }

        [HttpGet]
        public IActionResult Message()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            return View();
        }

        // --- CUSTOMER ---
        [HttpGet]
        public async Task<IActionResult> CustomerList(string? search, int page = 1)
        {
            int pageSize = 5;
            var result = await _customerService.GetCustomersAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerInfo(int id)
        {
            var customer = await _customerService.GetCustomerById(id);
            return View(customer);
        }

        // --- ROOM ---
        [HttpGet]
        public async Task<IActionResult> RoomList(string? search, int page = 1)
        {
            int pageSize = 5;
            var result = await _roomService.GetAllRoomsAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        // --- BOOKING ---
        [HttpGet]
        public async Task<IActionResult> BookingStatusList(BookingStatus? search, int page = 1)
        {
            int pageSize = 5;
            var result = await _bookingService.GetAllBookings(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> BookingDetail(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> BookingDetail(int id, string? status)
        {
            await _bookingService.BookingUpdateStatusAsync(id, status);
            return RedirectToAction("BookingStatusList", "Staff");
        }

        [HttpGet]
        public async Task<IActionResult> CheckIn(int id)
        {
            await _bookingService.CheckInAsync(id);
            TempData["Message"] = "Nhận phòng thành công!";
            return RedirectToAction("BookingStatusList", "Staff");
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            int days = (booking.ExpectedCheckOut.Date - booking.ExpectedCheckIn.Date).Days;
            int numberOfDays = days > 0 ? days : 1;

            decimal roomPrice = (booking.Room?.Price ?? 0) * numberOfDays;
            decimal serviceTotal = booking.Services?.Sum(s => s?.Price ?? 0) ?? 0;
            decimal deposit = booking.Deposit ?? 0;
            decimal totalAmount = roomPrice + serviceTotal - deposit;

            ViewBag.ServiceTotal = serviceTotal;
            ViewBag.TotalToPay = totalAmount > 0 ? totalAmount : 0;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckOut(int id, string paymentMethod)
        {
            await _bookingService.CheckOutAsync(id, paymentMethod);
            TempData["Message"] = "Trả phòng và thanh toán thành công!";
            return RedirectToAction("BookingStatusList", "Staff");
        }

        [HttpGet]
        public async Task<IActionResult> BookingDirect(int id)
        {
            var room = await _roomService.GetRoomById(id);
            var services = await _serviceHotel.GetAllAsync();
            ViewBag.Service = services;
            if (room == null) return NotFound();

            var model = new DirectBookingViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomTypeName,
                Price = room.Price,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                NumberOfPeople = 1
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BookingDirect(DirectBookingViewModel model)
        {
            await _bookingService.CreateBookingDirectAsync(model);
            return RedirectToAction("BookingStatusList", "Staff");
        }

        // --- INVOICE ---
        [HttpGet]
        public async Task<IActionResult> InvoiceList(string? search, int page = 1)
        {
            int pageSize = 5;
            var invoices = await _invoiceService.GetAllInvoicesAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> InvoiceDetail(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> ViewFeedback(string? search, int page = 1)
        {
            int pageSize = 5;
            var result = await _feedbackService.GetAllFeedback(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> FeedbackDetail(int id)
        {
            var result = await _feedbackService.GetFeedbackById(id);
            return View(result);
        }
    }
}