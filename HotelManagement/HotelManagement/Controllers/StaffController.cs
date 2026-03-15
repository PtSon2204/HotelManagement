using HotelManagement.Models.Entities;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class StaffController : Controller
    {
        private readonly CustomerService _customerService;
        private BookingServiceHanlde _bookingService;

        public StaffController(CustomerService service, BookingServiceHanlde bookingService)
        {
            _customerService = service;
            _bookingService = bookingService;
        }
        public IActionResult Index()
        {
            ViewBag.NumberOfCustomers = _customerService.CountCustomer();
            ViewBag.NumberOfBookings = _bookingService.NumberOfBookings();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CustomerList(string? search, int page = 1)
        {
            int pageSize = 5;

            var result = await _customerService.GetCustomersAsync(search, page, pageSize);

            ViewBag.Search = search;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> BookingStatusList(BookingStatus? search, int page = 1)
        {
            int pageSize = 5;

            var result = await _bookingService.GetAllBookings(search, page, pageSize);

            ViewBag.Search = search;
            return View(result);
        }
    }
}
