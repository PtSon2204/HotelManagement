using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class ServiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
