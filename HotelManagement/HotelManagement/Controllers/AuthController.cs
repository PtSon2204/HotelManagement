using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
