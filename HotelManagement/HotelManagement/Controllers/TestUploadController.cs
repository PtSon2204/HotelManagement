using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class TestUploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public TestUploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IFormFile? imageFile)
        {
            ViewBag.Status = "Chưa chọn file.";

            if (imageFile == null || imageFile.Length == 0)
            {
                ViewBag.Status = "Vui lòng chọn ảnh để tải lên.";
                return View();
            }

            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                ViewBag.Status = "Chỉ chấp nhận ảnh JPG, JPEG, PNG, GIF hoặc WEBP.";
                return View();
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "test-uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            ViewBag.Status = "Tải ảnh thành công.";
            ViewBag.FileName = imageFile.FileName;
            ViewBag.FileSize = imageFile.Length;
            ViewBag.SavedPath = $"/images/test-uploads/{fileName}";

            return View();
        }
    }
}
