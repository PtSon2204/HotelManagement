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

        public StaffController(CustomerService service, BookingServiceHanlde bookingService, RoomService roomService)
        {
            _customerService = service;
            _bookingService = bookingService;
            _roomService = roomService;
        }
        public IActionResult Index()
        {
            ViewBag.NumberOfCustomers = _customerService.CountCustomer();
            ViewBag.NumberOfBookings = _bookingService.NumberOfBookings();
            ViewBag.NumberOfRooms = _roomService.CountRooms();
            return View();
        }

        //Customer
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


        //Room
        [HttpGet] 
        public async Task<IActionResult> RoomList(string? search, int page = 1)
        {
            int pageSize = 5;
            var result = await _roomService.GetAllRoomsAsync(search, page, pageSize);
            ViewBag.Search = search;

            return View(result);
        }

        //Booking
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

            decimal roomPrice = booking.Room?.Price ?? 0;
            decimal deposit = booking.Deposit ?? 0;
            decimal totalAmount = roomPrice - deposit;

            // Truyền tổng tiền qua ViewBag để View hiển thị
            ViewBag.TotalToPay = totalAmount > 0 ? totalAmount : 0;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckOut(int id, string paymentMethod)
        {
            // Truyền thêm paymentMethod xuống Service
            await _bookingService.CheckOutAsync(id, paymentMethod);

            TempData["Message"] = "Trả phòng và thanh toán thành công!";
            return RedirectToAction("BookingStatusList", "Staff"); 
        }

        [HttpGet] 
        public async Task<IActionResult> BookingDirect(int id)
        {
            var room = await _roomService.GetRoomById(id);

            if (room == null)
            {
                return NotFound(); 
            }

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
    }
}
