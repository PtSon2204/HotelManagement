using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers // Đảm bảo đúng namespace của bạn
{
    [Area("Admin")] // Quan trọng: Phải có Attribute này để Layout nhận diện Area
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}