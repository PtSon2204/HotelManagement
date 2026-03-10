using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
