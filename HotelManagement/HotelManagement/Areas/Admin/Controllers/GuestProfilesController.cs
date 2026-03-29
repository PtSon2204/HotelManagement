using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class GuestProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.GuestProfiles.Include(g => g.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(g =>
                    g.FullName.ToLower().Contains(s) ||
                    g.Phone.Contains(s) ||
                    (g.Email != null && g.Email.ToLower().Contains(s)) ||
                    (g.IdCard != null && g.IdCard.Contains(s)));
            }

            int total = await query.CountAsync();
            var items = await query
                .OrderBy(g => g.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TotalCount = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            return View(items);
        }
    }
}
