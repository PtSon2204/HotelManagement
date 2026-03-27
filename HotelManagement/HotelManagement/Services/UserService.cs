using System.Security.Cryptography;
using System.Text;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserViewModel>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserViewModel
            {
                UserId = u.UserId,
                Username = u.Username,
                Password = string.Empty,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName,
                FullName = u.FullName,
            }).ToList();
        }

        public async Task<UserViewModel?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var roles = await _userRepository.GetRolesAsync();

            return new UserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                FullName = user.FullName,
                Roles = roles.Select(r => new RoleLookupItem
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                }).ToList()
            };
        }

        public async Task<List<RoleLookupItem>> GetRoleLookupAsync()
        {
            var roles = await _userRepository.GetRolesAsync();
            return roles.Select(r => new RoleLookupItem
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();
        }

        public async Task<(bool Success, string? Error)> CreateAsync(UserViewModel model)
        {
            if (await _userRepository.UsernameExistsAsync(model.Username))
                return (false, "Tên đăng nhập đã tồn tại.");

            var user = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password!),
                RoleId = model.RoleId,
                FullName = model.FullName ?? string.Empty
            };

            await _userRepository.CreateAsync(user);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(UserViewModel model)
        {
            var user = await _userRepository.GetByIdAsync(model.UserId);
            if (user == null) return (false, "Không tìm thấy tài khoản.");

            if (await _userRepository.UsernameExistsAsync(model.Username, model.UserId))
                return (false, "Tên đăng nhập đã tồn tại.");

            user.Username = model.Username;
            user.RoleId = model.RoleId;
            user.FullName = model.FullName ?? user.FullName;

            if (!string.IsNullOrWhiteSpace(model.Password))
                user.PasswordHash = HashPassword(model.Password);

            await _userRepository.UpdateAsync(user);
            return (true, null);
        }

        public async Task DeleteAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
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
