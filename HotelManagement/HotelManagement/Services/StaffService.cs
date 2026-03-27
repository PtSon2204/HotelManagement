using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    // Staff giờ là User với Role = "Staff"
    public class StaffService
    {
        private readonly StaffRepository _staffRepository;

        public StaffService(StaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<List<UserViewModel>> GetAllAsync()
        {
            var users = await _staffRepository.GetAllAsync();
            return users.Select(u => new UserViewModel
            {
                UserId = u.UserId,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName,
                FullName = u.FullName,
            }).ToList();
        }

        public async Task<UserViewModel?> GetByIdAsync(int id)
        {
            var user = await _staffRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                FullName = user.FullName,
            };
        }

        public async Task DeleteAsync(int id)
        {
            await _staffRepository.DeleteAsync(id);
        }
    }
}
