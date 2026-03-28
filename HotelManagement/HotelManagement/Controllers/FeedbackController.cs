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

        // ────────────────────────────────────────────────────────────────────────────────
        // GET: /Feedback/Create?bookingId=X
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create(int bookingId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LoginRegister", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("LoginRegister", "Account");

            // Chỉ cho phép feedback khi booking đã CheckedOut và thuộc về user
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == user.UserId);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đặt phòng.";
                return RedirectToAction("MyBookings", "Booking");
            }

            if (booking.Status != "CheckedOut" && booking.Status != "Đã trả phòng")
            {
                TempData["Error"] = "Chỉ có thể đánh giá sau khi đã trả phòng.";
                return RedirectToAction("MyBookings", "Booking");
            }

            // Kiểm tra đã feedback cho booking này chưa
            bool alreadyReviewed = await _context.Feedbacks
                .AnyAsync(f => f.UserId == user.UserId && f.RoomId == booking.RoomId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "Bạn đã gửi đánh giá cho phòng này rồi.";
                return RedirectToAction("MyBookings", "Booking");
            }

            ViewBag.Booking = booking;
            ViewBag.BookingId = bookingId;
            return View();
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // POST: /Feedback/Create
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookingId, int rating, string? comment)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LoginRegister", "Account");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("LoginRegister", "Account");

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Đánh giá không hợp lệ (1-5 sao).";
                return RedirectToAction("MyBookings", "Booking");
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == user.UserId);

            if (booking == null || (booking.Status != "CheckedOut" && booking.Status != "Đã trả phòng"))
            {
                TempData["Error"] = "Không thể gửi đánh giá cho đặt phòng này.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var feedback = new Feedback
            {
                UserId       = user.UserId,
                RoomId       = booking.RoomId,
                Rating       = rating,
                Comment      = comment ?? "",
                FeedbackDate = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["BookingSuccess"] = "🌟 Cảm ơn bạn đã gửi đánh giá! Ý kiến của bạn giúp chúng tôi phục vụ tốt hơn.";
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}
