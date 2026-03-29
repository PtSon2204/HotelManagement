using HotelManagement.Models.ViewModels;
using HotelManagement.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Filters;
using HotelManagement.Models.Entities;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? roleId)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            var users = await query.ToListAsync();
            var viewModels = users.Select(u => new UserViewModel
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName
            }).ToList();

            ViewBag.Roles = await _context.Roles.Select(r => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.RoleName
            }).ToListAsync();
            
            ViewBag.CurrentRoleId = roleId;

            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            var roles = await _context.Roles.Select(r => new RoleLookupItem { RoleId = r.RoleId, RoleName = r.RoleName }).ToListAsync();
            var model = new UserViewModel
            {
                Roles = roles
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
                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = model.Password!, // Assuming simple storage or hashing if needed (no service change)
                    FullName = string.IsNullOrWhiteSpace(model.FullName) ? model.Username : model.FullName,
                    RoleId = model.RoleId
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }

            model.Roles = await _context.Roles.Select(r => new RoleLookupItem { RoleId = r.RoleId, RoleName = r.RoleName }).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            var currentUsername = HttpContext.Session.GetString("Username");
            if (u.Username == currentUsername)
                return RedirectToAction(nameof(Index));

            var model = new UserViewModel
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                RoleId = u.RoleId,
                Roles = await _context.Roles.Select(r => new RoleLookupItem { RoleId = r.RoleId, RoleName = r.RoleName }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var u = await _context.Users.FindAsync(model.UserId);
            if (u == null) return NotFound();

            var currentUsername = HttpContext.Session.GetString("Username");
            if (u.Username == currentUsername)
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
                u.FullName = string.IsNullOrWhiteSpace(model.FullName) ? model.Username : model.FullName;
                u.RoleId = model.RoleId;
                if (!string.IsNullOrWhiteSpace(model.Password))
                    u.PasswordHash = model.Password;

                _context.Users.Update(u);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }

            model.Roles = await _context.Roles.Select(r => new RoleLookupItem { RoleId = r.RoleId, RoleName = r.RoleName }).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            var currentUsername = HttpContext.Session.GetString("Username");
            if (u.Username == currentUsername)
                return RedirectToAction(nameof(Index));

            try
            {
                _context.Users.Remove(u);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tài khoản thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa tài khoản này vì họ đang có lịch sử đặt phòng, đánh giá, hoặc hóa đơn.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
