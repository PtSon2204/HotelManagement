using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomsController : Controller
    {
        private readonly RoomService _roomService;
        private readonly IWebHostEnvironment _env;

        public RoomsController(RoomService roomService, IWebHostEnvironment env)
        {
            _roomService = roomService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllAsync();
            return View(rooms);
        }

        public IActionResult Create()
        {
            var model = new RoomViewModel { Status = "Available" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomViewModel model, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                RoomViewModel created;
                try
                {
                    created = await _roomService.CreateAsync(model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var urls = new List<string>();
                    try
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file == null || file.Length == 0) continue;
                            urls.Add(await SaveRoomImageAsync(file));
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        return View(model);
                    }

                    if (urls.Count > 0)
                        await _roomService.AddImagesAsync(created.RoomId, urls);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _roomService.GetByIdAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomViewModel model, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdateAsync(model);

                var deleteIds = model.DeleteImageIds ?? new List<int>();
                if (deleteIds.Count > 0)
                {
                    var existing = await _roomService.GetImagesByRoomIdAsync(model.RoomId);
                    var toDelete = existing.Where(i => deleteIds.Contains(i.ImageId)).ToList();
                    await _roomService.DeleteImagesAsync(model.RoomId, deleteIds);
                    foreach (var img in toDelete)
                        TryDeletePhysicalFile(img.Url);
                }

                if (imageFiles != null && imageFiles.Count > 0)
                {
                    var urls = new List<string>();
                    try
                    {
                        foreach (var file in imageFiles)
                        {
                            if (file == null || file.Length == 0) continue;
                            urls.Add(await SaveRoomImageAsync(file));
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        model = await _roomService.GetByIdAsync(model.RoomId) ?? model;
                        return View(model);
                    }

                    if (urls.Count > 0)
                        await _roomService.AddImagesAsync(model.RoomId, urls);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existingImages = await _roomService.GetImagesByRoomIdAsync(id);
            await _roomService.DeleteAsync(id);
            foreach (var img in existingImages)
                TryDeletePhysicalFile(img.Url);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveRoomImageAsync(IFormFile imageFile)
        {
            const int maxSizeBytes = 2 * 1024 * 1024;
            if (imageFile.Length > maxSizeBytes)
                throw new InvalidOperationException("Kich thuoc anh toi da la 2MB.");

            var safeFileName = Path.GetFileName(imageFile.FileName);
            var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Chi chap nhan anh JPG, JPEG, PNG, GIF hoac WEBP.");

            var contentType = (imageFile.ContentType ?? string.Empty).ToLowerInvariant();
            var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "image/jpeg", "image/png", "image/gif", "image/webp" };

            if (!allowedContentTypes.Contains(contentType))
                throw new InvalidOperationException("Dinh dang anh khong hop le.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "rooms");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/rooms/{fileName}";
        }

        private void TryDeletePhysicalFile(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.StartsWith("/images/rooms/", StringComparison.OrdinalIgnoreCase)) return;

            var relative = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relative);
            var roomsRoot = Path.Combine(_env.WebRootPath, "images", "rooms");
            var fullNormalized = Path.GetFullPath(fullPath);
            var rootNormalized = Path.GetFullPath(roomsRoot) + Path.DirectorySeparatorChar;
            if (!fullNormalized.StartsWith(rootNormalized, StringComparison.OrdinalIgnoreCase)) return;

            if (System.IO.File.Exists(fullNormalized))
                System.IO.File.Delete(fullNormalized);
        }
    }
}
