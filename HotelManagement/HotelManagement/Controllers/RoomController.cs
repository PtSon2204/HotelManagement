using HotelManagement.Context;
using HotelManagement.Models.Entities;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class RoomController : Controller
    {
        private readonly RoomService _roomService;
        private readonly ApplicationDbContext _context;

        public RoomController(RoomService roomService, ApplicationDbContext context)
        {
            _roomService = roomService;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Search = search;
            ViewBag.Status = status;

            const int pageSize = 8;
            var result = await _roomService.GetAllRoomsAsync(search, status, page, pageSize);

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            var canSubmitFeedback = false;
            string? feedbackEligibilityMessage = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                feedbackEligibilityMessage = "Bạn cần đăng nhập bằng tài khoản khách hàng để gửi feedback.";
            }
            else if (!string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                feedbackEligibilityMessage = "Chỉ tài khoản khách hàng mới có thể gửi feedback.";
            }
            else
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    feedbackEligibilityMessage = "Không tìm thấy tài khoản để gửi feedback.";
                }
                else
                {
                    canSubmitFeedback = await _context.Bookings.AnyAsync(b =>
                        b.UserId == user.UserId &&
                        b.RoomId == id &&
                        b.ActualCheckIn.HasValue &&
                        b.ActualCheckOut.HasValue);

                    if (!canSubmitFeedback)
                    {
                        feedbackEligibilityMessage = "Bạn chỉ có thể đánh giá sau khi đã đặt, check-in, sử dụng và check-out phòng này.";
                    }
                }
            }

            ViewBag.CanSubmitFeedback = canSubmitFeedback;
            ViewBag.FeedbackEligibilityMessage = feedbackEligibilityMessage;

            var room = await _roomService.GetRoomById(id);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeedback(int id, int rating, string? comment)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");
            var normalizedComment = (comment ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["FeedbackError"] = "Bạn cần đăng nhập để gửi đánh giá.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            if (!string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                TempData["FeedbackError"] = "Chỉ khách hàng mới có thể gửi đánh giá.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["FeedbackError"] = "Điểm đánh giá phải từ 1 đến 5 sao.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            if (string.IsNullOrWhiteSpace(normalizedComment))
            {
                TempData["FeedbackError"] = "Nội dung feedback không được để trống hoặc chỉ chứa khoảng trắng.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var roomExists = await _context.Rooms.AnyAsync(r => r.RoomId == id);
            if (!roomExists)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                TempData["FeedbackError"] = "Không tìm thấy tài khoản để gửi đánh giá.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var hasCompletedStay = await _context.Bookings.AnyAsync(b =>
                b.UserId == user.UserId &&
                b.RoomId == id &&
                b.ActualCheckIn.HasValue &&
                b.ActualCheckOut.HasValue);

            if (!hasCompletedStay)
            {
                TempData["FeedbackError"] = "Bạn chỉ có thể đánh giá sau khi đã đặt, check-in, sử dụng và check-out phòng này.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var feedback = new Feedback
            {
                RoomId = id,
                UserId = user.UserId,
                Rating = rating,
                Comment = normalizedComment,
                FeedbackDate = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["FeedbackSuccess"] = "Gửi đánh giá thành công.";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }
}
