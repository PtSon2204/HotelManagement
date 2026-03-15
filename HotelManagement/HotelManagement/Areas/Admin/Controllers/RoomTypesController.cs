using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomTypesController : Controller
    {
        private readonly RoomTypeService _roomTypeService;

        public RoomTypesController(RoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var roomTypes = await _roomTypeService.GetAllAsync();
            return View(roomTypes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _roomTypeService.CreateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi lưu: {ex.Message}");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _roomTypeService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _roomTypeService.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi lưu: {ex.Message}");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _roomTypeService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
