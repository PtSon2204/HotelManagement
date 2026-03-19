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

        public async Task<IActionResult> Index()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var rooms = await _roomService.GetAllAsync();
            return View(rooms);
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
