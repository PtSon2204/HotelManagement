using HotelManagement.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.NumberOfCustomers = await _context.Users.CountAsync(u => u.Role != null && u.Role.RoleName == "Customer");
            ViewBag.NumberOfBookings = await _context.Bookings.CountAsync();
            ViewBag.NumberOfRooms = await _context.Rooms.CountAsync(r => r.Status == "Available" || r.Status == "Tr?ng");
            ViewBag.NumberOfServices = await _context.Services.CountAsync();
            ViewBag.NumberOfFeedbacks = await _context.Feedbacks.CountAsync();

            return View();
        }
    }
}
