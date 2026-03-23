using HotelManagement.Services;
using HotelManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class RoomController : Controller
    {
        private readonly RoomService _roomService;

        public RoomController(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 8;

            var result = await _roomService.GetAllRoomsAsync(null, page, pageSize);

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var room = await _roomService.GetRoomById(id);
            if (room == null) return NotFound();

            return View(room);
        }
    }
}
