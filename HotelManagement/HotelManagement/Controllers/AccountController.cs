using HotelManagement.Context;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HotelManagement.Services;

namespace HotelManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSenderService _emailSenderService;

        public AccountController(ApplicationDbContext context, EmailSenderService emailSenderService)
        {
            _context = context;
            _emailSenderService = emailSenderService;
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

            var loginValue = model.Username?.Trim();
            var passwordHash = Program.Hash(model.Password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email == loginValue &&
                    u.PasswordHash == passwordHash);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Gmail hoặc mật khẩu không đúng.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(model, new RegisterViewModel()));
            }

            var activation = await _context.AccountActivations
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(a => a.UserId == user.UserId);

            if (activation != null && !activation.IsVerified)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn chưa được kích hoạt. Vui lòng nhập mã OTP đã gửi về email.");
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
        [ValidateAntiForgeryToken]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleResponse))
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["GoogleLoginError"] = "Dang nhap Google that bai.";
                return RedirectToAction(nameof(LoginRegister));
            }

            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var fullName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name");

            if (string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["GoogleLoginError"] = "Khong lay duoc email tu tai khoan Google.";
                return RedirectToAction(nameof(LoginRegister));
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
                if (customerRole == null)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["GoogleLoginError"] = "He thong chua cau hinh role Customer.";
                    return RedirectToAction(nameof(LoginRegister));
                }

                var baseUsername = BuildUsernameFromEmail(email);
                var username = await GenerateUniqueUsernameAsync(baseUsername);

                user = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(Guid.NewGuid().ToString("N")),
                    RoleId = customerRole.RoleId,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? username : fullName,
                    Email = email
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _context.AccountActivations.Add(new AccountActivation
                {
                    UserId = user.UserId,
                    Email = email,
                    OtpCode = "GOOGLE",
                    ExpiresAt = DateTime.Now.AddYears(10),
                    IsVerified = true,
                    CreatedAt = DateTime.Now,
                    VerifiedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();

                user.Role = customerRole;
            }
            else
            {
                var activation = await _context.AccountActivations
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefaultAsync(a => a.UserId == user.UserId);

                if (activation == null)
                {
                    _context.AccountActivations.Add(new AccountActivation
                    {
                        UserId = user.UserId,
                        Email = email,
                        OtpCode = "GOOGLE",
                        ExpiresAt = DateTime.Now.AddYears(10),
                        IsVerified = true,
                        CreatedAt = DateTime.Now,
                        VerifiedAt = DateTime.Now
                    });
                }
                else if (!activation.IsVerified)
                {
                    activation.IsVerified = true;
                    activation.VerifiedAt = DateTime.Now;
                    activation.Email = email;
                }

                await _context.SaveChangesAsync();
            }

            var roleName = user.Role?.RoleName ?? "Customer";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", roleName);

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

            var existingEmail = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (existingEmail)
            {
                ModelState.AddModelError(string.Empty, "Email da duoc su dung.");
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
                FullName = model.Username,
                Email = model.Email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var activation = new AccountActivation
            {
                UserId = user.UserId,
                Email = model.Email,
                OtpCode = otpCode,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                IsVerified = false
            };

            _context.AccountActivations.Add(activation);
            await _context.SaveChangesAsync();

            try
            {
                await _emailSenderService.SendOtpAsync(model.Email, model.Username, otpCode);
            }
            catch (Exception ex)
            {
                _context.AccountActivations.Remove(activation);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, $"Khong gui duoc email OTP: {ex.Message}");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), model));
            }

            TempData["RegisterSuccess"] = "Dang ky thanh cong. Vui long kiem tra email de lay ma OTP kich hoat tai khoan.";
            return RedirectToAction(nameof(VerifyOtp), new { username = user.Username });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string? username)
        {
            return View(new VerifyOtpViewModel
            {
                Username = username ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Khong tim thay tai khoan can kich hoat.");
                return View(model);
            }

            var activation = await _context.AccountActivations
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(a => a.UserId == user.UserId);

            if (activation == null)
            {
                ModelState.AddModelError(string.Empty, "Tai khoan nay khong co ma OTP kich hoat.");
                return View(model);
            }

            if (activation.IsVerified)
            {
                TempData["OtpSuccess"] = "Tai khoan da duoc kich hoat. Vui long dang nhap.";
                return RedirectToAction(nameof(LoginRegister));
            }

            if (activation.ExpiresAt < DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "Ma OTP da het han. Vui long dang ky lai hoac lien he quan tri vien.");
                return View(model);
            }

            if (!string.Equals(activation.OtpCode, model.OtpCode?.Trim(), StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Ma OTP khong dung.");
                return View(model);
            }

            activation.IsVerified = true;
            activation.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["OtpSuccess"] = "Kich hoat tai khoan thanh cong. Ban co the dang nhap ngay bay gio.";
            return RedirectToAction(nameof(LoginRegister));
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

            var normalizedEmail = model.Email.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Gmail chua duoc dang ky trong he thong.");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), new RegisterViewModel()));
            }

            var newPassword = GenerateRandomPassword();
            user.PasswordHash = HashPassword(newPassword);

            try
            {
                await _emailSenderService.SendNewPasswordAsync(normalizedEmail, user.Username, newPassword);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Khong gui duoc mat khau moi qua email: {ex.Message}");
                return View("LoginRegister", new Tuple<LoginViewModel, RegisterViewModel>(new LoginViewModel(), new RegisterViewModel()));
            }

            TempData["ForgotPasswordSuccess"] = "He thong da gui mat khau moi ve Gmail da dang ky.";
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return RedirectToAction("LoginRegister");

            // Email is view-only in profile and must not be changed from this form.
            model.Email = user.Email;

            if (!ModelState.IsValid)
                return View(model);

            // Cập nhật trực tiếp vào User (không cần Customer table nữa)
            user.FullName = model.FullName;
            user.Gender = model.Gender;
            user.IDCard = model.Idcard;
            user.Address = model.Address;
            user.Nationality = model.Nationality;
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

        private static string BuildUsernameFromEmail(string email)
        {
            var username = email.Split('@')[0];
            username = new string(username.Where(char.IsLetterOrDigit).ToArray());
            return string.IsNullOrWhiteSpace(username) ? "googleuser" : username;
        }

        private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
        {
            var username = baseUsername;
            var suffix = 1;

            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = $"{baseUsername}{suffix}";
                suffix++;
            }

            return username;
        }

        private static string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var buffer = new char[10];

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            return new string(buffer);
        }
    }
}
