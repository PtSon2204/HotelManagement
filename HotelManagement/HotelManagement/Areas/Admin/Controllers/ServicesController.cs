using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

using HotelManagement.Filters;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var services = _context.Services
                .Select(s => new Models.ViewModels.ServiceViewModel
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price,
                    IsActive = s.IsActive
                }).ToList();

            return View(services);
        }

        public IActionResult Create()
        {
            return View(new Models.ViewModels.ServiceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.ViewModels.ServiceViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = new Models.Entities.Service
            {
                Name = model.Name,
                Price = model.Price,
                IsActive = model.IsActive
            };
            _context.Services.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null) return NotFound();
            var viewModel = new Models.ViewModels.ServiceViewModel
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price,
                IsActive = service.IsActive
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.ViewModels.ServiceViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var entity = _context.Services.Find(model.ServiceId);
            if (entity == null) return NotFound();
            entity.Name = model.Name;
            entity.Price = model.Price;
            entity.IsActive = model.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = _context.Services.Find(id);
            if (entity != null)
            {
                entity.IsActive = !(entity.IsActive ?? true); // Handle null as true initially
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã {(entity.IsActive == true ? "kích hoạt" : "vô hiệu hóa")} dịch vụ {entity.Name}!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
