using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.Include(r => r.Users).ToListAsync();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm quyền thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role role)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật quyền thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.RoleId == id);
            if (role != null)
            {
                if (role.Users != null && role.Users.Any())
                {
                    TempData["Error"] = "Không thể xóa quyền đang được gán cho người dùng!";
                }
                else
                {
                    _context.Roles.Remove(role);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa quyền thành công!";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
