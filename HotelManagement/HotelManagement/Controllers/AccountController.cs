using HotelManagement.Context;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

            var passwordHash = Program.Hash(model.Password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == passwordHash);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(model, new RegisterViewModel()));
            }

            var roleName = user.Role?.RoleName ?? "Customer";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", roleName);

            if (roleName == "Admin")
                return RedirectToAction("Index", "Rooms", new { area = "Admin" });

            if (roleName == "Staff")
                return RedirectToAction("Index", "Staff");

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

            // Tìm Role Customer
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (customerRole == null)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống chưa cấu hình Role. Vui lòng liên hệ Admin.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), model));
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                RoleId = customerRole.RoleId,
                FullName = model.Username // default FullName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", "Customer");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("LoginRegister");

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("LoginRegister");

            if (!ModelState.IsValid)
                return View(model);

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

        //profile   

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("LoginRegister");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("LoginRegister");

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Gender = user.Gender,
                Idcard = user.IDCard,
                Address = user.Address,
                Nationality = user.Nationality,
                Email = user.Email,
                Phone = user.Phone
            };

            return View(model);
        }

        // Cập nhật thông tin cá nhân

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("LoginRegister");

            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("LoginRegister");

            // Cập nhật trực tiếp vào User (không cần Customer table nữa)
            user.FullName = model.FullName;
            user.Gender = model.Gender;
            user.IDCard = model.Idcard;
            user.Address = model.Address;
            user.Nationality = model.Nationality;
            user.Email = model.Email;
            user.Phone = model.Phone;

            await _context.SaveChangesAsync();

            TempData["ProfileSuccess"] = "Cap nhat thong tin ca nhan thanh cong.";
            return RedirectToAction("Profile");
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
