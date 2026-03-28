using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("LoginRegister", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int rating, string comment)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null) return RedirectToAction("LoginRegister", "Account");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản. Vui lòng đăng nhập lại.";
                return RedirectToAction("LoginRegister", "Account");
            }

            var feedback = new Feedback
            {
                UserId = user.UserId,
                RoomId = 1, // default fallback - ideally passed from the booking
                Rating = rating,
                Comment = comment,
                FeedbackDate = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = "Cảm ơn bạn đã gửi đánh giá! Xin hẹn gặp lại quý khách.";
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}
