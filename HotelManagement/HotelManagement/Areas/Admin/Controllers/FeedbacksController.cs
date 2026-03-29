using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class FeedbacksController : Controller
    {
        private readonly FeedbackRepository _feedbackRepository;
        private readonly ApplicationDbContext _context;

        public FeedbacksController(FeedbackRepository feedbackRepository, ApplicationDbContext context)
        {
            _feedbackRepository = feedbackRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var result = await _feedbackRepository.GetAllFeedback(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa phản hồi thành công!";
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
