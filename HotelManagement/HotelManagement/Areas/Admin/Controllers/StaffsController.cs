//using HotelManagement.Models.ViewModels;
//using HotelManagement.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace HotelManagement.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class StaffsController : Controller
//    {
//        private readonly StaffService _staffService;

//        public StaffsController(StaffService staffService)
//        {
//            _staffService = staffService;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var staffs = await _staffService.GetAllAsync();
//            return View(staffs);
//        }

//        public IActionResult Create()
//        {
//            return View(new UserViewModel());
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(UserViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                await _staffService.DeleteAsync(0); // placeholder - not used
//                return RedirectToAction(nameof(Index));
//            }
//            return View(model);
//        }

//        public async Task<IActionResult> Edit(int id)
//        {
//            var model = await _staffService.GetByIdAsync(id);
//            if (model == null) return NotFound();
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(UserViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                return RedirectToAction(nameof(Index));
//            }
//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            await _staffService.DeleteAsync(id);
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
