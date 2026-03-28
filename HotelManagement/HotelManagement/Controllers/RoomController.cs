using HotelManagement.Context;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class RoomController : Controller
    {
        private readonly RoomService    _roomService;
        private readonly ApplicationDbContext _context;

        public RoomController(RoomService roomService, ApplicationDbContext context)
        {
            _roomService = roomService;
            _context     = context;
        }

        // GET: /Room/Index?page=1
        public async Task<IActionResult> Index(int page = 1)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            const int pageSize = 8;
            var result = await _roomService.GetAllRoomsAsync(null, page, pageSize);
            return View(result);
        }

        // GET: /Room/Detail?id=X
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var room = await _roomService.GetRoomById(id);
            if (room == null) return NotFound();

            return View(room);
        }

        // GET: /Room/GetRoomPrice?roomId=X  (AJAX – used by booking form)
        [HttpGet]
        public async Task<IActionResult> GetRoomPrice(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return Json(new { price = 0, capacity = 0 });
            return Json(new { price = room.Price, capacity = room.Capacity });
        }
    }
}
