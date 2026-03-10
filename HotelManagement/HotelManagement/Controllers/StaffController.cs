using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
