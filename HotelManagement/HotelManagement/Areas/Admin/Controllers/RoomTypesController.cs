using HotelManagement.Models.ViewModels;
using HotelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomTypesController : Controller
    {
        private readonly RoomTypeService _roomTypeService;
        private readonly IWebHostEnvironment _env;

        public RoomTypesController(RoomTypeService roomTypeService, IWebHostEnvironment env)
        {
            _roomTypeService = roomTypeService;
            _env = env;
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
        public async Task<IActionResult> UploadImageBase64([FromBody] UploadImageBase64Request request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.DataUrl))
                    return BadRequest(new { error = "Vui long chon anh de tai len." });

                var imagePath = await SaveBase64ImageAsync(request.DataUrl, request.FileName);
                return Json(new { path = imagePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _roomTypeService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveBase64ImageAsync(string dataUrl, string? originalFileName)
        {
            const int maxSizeBytes = 2 * 1024 * 1024;

            var commaIndex = dataUrl.IndexOf(',');
            if (commaIndex <= 0)
                throw new InvalidOperationException("Du lieu anh khong hop le.");

            var metadata = dataUrl[..commaIndex];
            var base64 = dataUrl[(commaIndex + 1)..];

            if (!metadata.Contains(";base64", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Du lieu anh khong dung dinh dang base64.");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Noi dung anh khong hop le.");
            }

            if (bytes.Length == 0)
                throw new InvalidOperationException("Anh tai len khong hop le.");

            if (bytes.Length > maxSizeBytes)
                throw new InvalidOperationException("Kich thuoc anh toi da la 2MB.");

            var ext = GetValidImageExtension(metadata, originalFileName);

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "roomtypes");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
            return $"/images/roomtypes/{fileName}";
        }

        private static string GetValidImageExtension(string metadata, string? originalFileName)
        {
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp"
            };

            string? mimeType = null;
            if (metadata.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var semicolonIndex = metadata.IndexOf(';');
                if (semicolonIndex > 5)
                    mimeType = metadata[5..semicolonIndex].Trim();
            }

            var extFromMime = mimeType?.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => null
            };

            var safeFileName = Path.GetFileName(originalFileName ?? string.Empty);
            var extFromName = Path.GetExtension(safeFileName)?.ToLowerInvariant();

            var ext = !string.IsNullOrWhiteSpace(extFromName) ? extFromName : extFromMime;
            if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Chi chap nhan anh JPG, JPEG, PNG, GIF hoac WEBP.");

            if (extFromMime != null && !string.Equals(extFromMime, ext, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Dinh dang anh khong trung khop.");

            return ext;
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                throw new InvalidOperationException("Chỉ chấp nhận ảnh JPG, JPEG, PNG, GIF hoặc WEBP.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "roomtypes");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/images/roomtypes/{fileName}";
        }

        public sealed class UploadImageBase64Request
        {
            public string DataUrl { get; set; } = string.Empty;
            public string? FileName { get; set; }
        }
    }
}
