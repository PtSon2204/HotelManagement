using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        public async Task<IActionResult> Create()
        {
            var model = new UserViewModel
            {
                Role = "Staff",
                Staffs = await _userService.GetStaffLookupAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(UserViewModel.Password), "Vui lòng nhập mật khẩu");

            if (model.Password != model.ConfirmPassword)
                ModelState.AddModelError(nameof(UserViewModel.ConfirmPassword), "Mật khẩu xác nhận không khớp");

            if (ModelState.IsValid)
            {
                var result = await _userService.CreateAsync(model);
                if (result.Success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, result.Error!);
            }

            model.Staffs = await _userService.GetStaffLookupAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _userService.GetByIdAsync(id);
            if (model == null) return NotFound();

            var currentUsername = HttpContext.Session.GetString("Username");
            if (model.Username == currentUsername)
                return RedirectToAction(nameof(Index));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var existing = await _userService.GetByIdAsync(model.UserId);
            if (existing == null) return NotFound();

            var currentUsername = HttpContext.Session.GetString("Username");
            if (existing.Username == currentUsername)
                return RedirectToAction(nameof(Index));

            ModelState.Remove(nameof(UserViewModel.Password));
            ModelState.Remove(nameof(UserViewModel.ConfirmPassword));

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password.Length < 8 || model.Password.Length > 100)
                    ModelState.AddModelError(nameof(UserViewModel.Password), "Mật khẩu phải từ 8 đến 100 ký tự");

                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError(nameof(UserViewModel.ConfirmPassword), "Mật khẩu xác nhận không khớp");
            }

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateAsync(model);
                if (result.Success)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError(string.Empty, result.Error!);
            }

            model.Staffs = await _userService.GetStaffLookupAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _userService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUsername = HttpContext.Session.GetString("Username");
            if (existing.Username == currentUsername)
                return RedirectToAction(nameof(Index));

            await _userService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
