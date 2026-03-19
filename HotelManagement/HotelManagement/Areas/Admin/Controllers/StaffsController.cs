using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffsController : Controller
    {
        private readonly StaffService _staffService;

        public StaffsController(StaffService staffService)
        {
            _staffService = staffService;
        }

        public async Task<IActionResult> Index()
        {
            var staffs = await _staffService.GetAllAsync();
            return View(staffs);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffViewModel model)
        {
            ValidateAge(model);

            if (ModelState.IsValid)
            {
                await _staffService.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _staffService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffViewModel model)
        {
            ValidateAge(model);

            if (ModelState.IsValid)
            {
                await _staffService.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private void ValidateAge(StaffViewModel model)
        {
            if (model.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - model.DateOfBirth.Value.Year;
                if (model.DateOfBirth.Value > today.AddYears(-age))
                    age--;

                if (age < 18)
                    ModelState.AddModelError(nameof(StaffViewModel.DateOfBirth), "Nhân viên phải đủ 18 tuổi trở lên");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _staffService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
