using HotelManagement.Models.Entities;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingServiceM _bookingService;

        public BookingController(BookingServiceM bookingService)
        {
            _bookingService = bookingService;
        }

        public IActionResult Index()
        {
            ViewBag.NumberOfBookings =  _bookingService.NumberOfBookings();

            return View();
        }

        public async Task<IActionResult> ConfirmList()
        {
            var bookings = await _bookingService.GetPedingBookings();

            if (!bookings.Any())
            {
                ViewBag.Message = "No pending bookings found";
            }
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int id, BookingStatus status)
        {
            await _bookingService.ConfirmBooking(id, status);

            return RedirectToAction("ConfirmList");
        }
    }
}
