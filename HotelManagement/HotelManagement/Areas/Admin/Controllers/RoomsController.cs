using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomsController : Controller
    {
        private readonly RoomService _roomService;

        public RoomsController(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllAsync();
            return View(rooms);
        }

        public async Task<IActionResult> Create()
        {
            var model = new RoomViewModel
            {
                RoomTypes = await _roomService.GetRoomTypesAsync(),
                Status = "Available"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _roomService.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            model.RoomTypes = await _roomService.GetRoomTypesAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _roomService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            model.RoomTypes = await _roomService.GetRoomTypesAsync();
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var model = await _roomService.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roomService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
