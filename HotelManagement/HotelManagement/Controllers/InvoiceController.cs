using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class InvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
