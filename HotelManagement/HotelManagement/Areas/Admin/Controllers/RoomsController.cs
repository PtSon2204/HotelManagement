using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using HotelManagement.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Filters;
using HotelManagement.Repositories;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class RoomsController : Controller
    {
        private readonly RoomRepository _roomRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public RoomsController(RoomRepository roomRepository, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _roomRepository = roomRepository;
            _env = env;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomRepository.GetAllAsync();
            var viewModels = rooms.Select(room => new RoomViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                Status = room.Status,
                RoomTypeName = room.RoomTypeName,
                Capacity = room.Capacity,
                Description = room.Description,
                IsActive = room.IsActive,
                Images = room.Images?.Select(i => new RoomImageItem { ImageId = i.ImageId, Url = i.Url }).ToList() ?? new List<RoomImageItem>(),
                ImageUrls = room.Images?.Select(i => i.Url).ToList() ?? new List<string>()
            }).ToList();
            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            return View(new RoomViewModel());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return NotFound();
            var vm = new RoomViewModel
            {
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                Status = room.Status,
                RoomTypeName = room.RoomTypeName,
                Capacity = room.Capacity,
                Description = room.Description,
                IsActive = room.IsActive,
                Images = room.Images?.Select(i => new RoomImageItem { ImageId = i.ImageId, Url = i.Url }).ToList() ?? new List<RoomImageItem>(),
                ImageUrls = room.Images?.Select(i => i.Url).ToList() ?? new List<string>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomViewModel model, List<IFormFile> imageFiles)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = new HotelManagement.Models.Entities.Room
            {
                RoomNumber = model.RoomNumber,
                Price = model.Price ?? 0m,
                Status = model.Status ?? "Available",
                RoomTypeName = model.RoomTypeName ?? string.Empty,
                Capacity = model.Capacity ?? 0,
                Description = model.Description,
                IsActive = model.IsActive
            };

            await _roomRepository.CreateAsync(entity);

            if (imageFiles != null && imageFiles.Count > 0)
            {
                var imageUrls = new List<string>();
                foreach (var file in imageFiles)
                {
                    // Save the file and add URL to imageUrls
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images/rooms", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    imageUrls.Add("/images/rooms/" + fileName);
                }
                await _roomRepository.AddImagesAsync(entity.RoomId, imageUrls);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomViewModel model, List<IFormFile> imageFiles)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = await _roomRepository.GetByIdAsync(model.RoomId);
            if (entity == null)
                return NotFound();

            entity.RoomNumber = model.RoomNumber;
            entity.Price = model.Price ?? entity.Price;
            entity.Status = model.Status ?? entity.Status;
            entity.RoomTypeName = model.RoomTypeName ?? entity.RoomTypeName;
            entity.Capacity = model.Capacity ?? entity.Capacity;
            entity.Description = model.Description;
            entity.IsActive = model.IsActive;

            // Handle delete images
            if (model.DeleteImageIds.Any())
            {
                await _roomRepository.DeleteImagesAsync(entity.RoomId, model.DeleteImageIds);
            }
            // Handle new images
            if (imageFiles != null && imageFiles.Count > 0)
            {
                var imageUrls = new List<string>();
                foreach (var file in imageFiles)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "images/rooms", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    imageUrls.Add("/images/rooms/" + fileName);
                }
                await _roomRepository.AddImagesAsync(entity.RoomId, imageUrls);
            }
            await _roomRepository.UpdateAsync(entity);
            return RedirectToAction(nameof(Index));
        }

        // Admin-only: View feedbacks for a specific room
        public async Task<IActionResult> Feedbacks(int roomId)
        {
            var room = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomId == roomId);
            if (room == null)
                return NotFound();

            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Where(f => f.RoomId == roomId)
                .OrderByDescending(f => f.FeedbackDate)
                .Select(f => new FeedbackViewModel
                {
                    FeedbackId = f.FeedbackId,
                    UserId = f.UserId,
                    FullName = f.User.FullName ?? "(Không tên)",
                    RoomId = f.RoomId,
                    RoomNumber = room.RoomNumber,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    FeedbackDate = f.FeedbackDate
                })
                .ToListAsync();

            ViewBag.RoomNumber = room.RoomNumber;
            ViewBag.RoomId = room.RoomId;
            return View("Feedbacks", feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room != null)
            {
                room.IsActive = !room.IsActive;
                await _roomRepository.UpdateAsync(room);
                TempData["Success"] = $"Đã {(room.IsActive ? "kích hoạt" : "vô hiệu hóa")} phòng {room.RoomNumber}!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
