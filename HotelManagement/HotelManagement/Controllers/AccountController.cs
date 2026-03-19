using System.Security.Cryptography;
using System.Text;
using HotelManagement.Context;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult LoginRegister()
        {
            return View(new Tuple<LoginViewModel, RegisterViewModel>(
                new LoginViewModel(),
                new RegisterViewModel()));
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(model, new RegisterViewModel()));
            }

            var passwordHash = HashPassword(model.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == passwordHash);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Ten dang nhap hoac mat khau khong dung.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(model, new RegisterViewModel()));
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), model));
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mat khau xac nhan khong khop.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), model));
            }

            var existingUser = await _context.Users.AnyAsync(u => u.Username == model.Username);
            if (existingUser)
            {
                ModelState.AddModelError(string.Empty, "Ten dang nhap da ton tai.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), model));
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("LoginRegister");
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("LoginRegister");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError(string.Empty, "Mat khau moi xac nhan khong khop.");
                return View(model);
            }

            var username = HttpContext.Session.GetString("Username")!;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Nguoi dung khong ton tai.");
                return View(model);
            }

            var currentHash = HashPassword(model.CurrentPassword);
            if (user.PasswordHash != currentHash)
            {
                ModelState.AddModelError(string.Empty, "Mat khau hien tai khong dung.");
                return View(model);
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["ChangePasswordSuccess"] = "Doi mat khau thanh cong.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), new RegisterViewModel()));
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError(string.Empty, "Mat khau moi xac nhan khong khop.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), new RegisterViewModel()));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Ten dang nhap khong ton tai.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), new RegisterViewModel()));
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["ForgotPasswordSuccess"] = "Dat lai mat khau thanh cong. Vui long dang nhap lai.";
            return RedirectToAction("LoginRegister");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

