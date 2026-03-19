using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class StaffService
    {
        private readonly StaffRepository _staffRepository;

        public StaffService(StaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<List<StaffViewModel>> GetAllAsync()
        {
            var staffs = await _staffRepository.GetAllAsync();
            return staffs.Select(s => new StaffViewModel
            {
                StaffId = s.StaffId,
                FullName = s.FullName,
                Gender = s.Gender,
                DateOfBirth = s.DateOfBirth,
                Address = s.Address,
                Email = s.Email,
                Phone = s.Phone,
                Image = s.Image
            }).ToList();
        }

        public async Task<StaffViewModel?> GetByIdAsync(int id)
        {
            var staff = await _staffRepository.GetByIdAsync(id);
            if (staff == null) return null;

            return new StaffViewModel
            {
                StaffId = staff.StaffId,
                FullName = staff.FullName,
                Gender = staff.Gender,
                DateOfBirth = staff.DateOfBirth,
                Address = staff.Address,
                Email = staff.Email,
                Phone = staff.Phone,
                Image = staff.Image
            };
        }

        public async Task<StaffViewModel> CreateAsync(StaffViewModel model)
        {
            var staff = new Staff
            {
                FullName = model.FullName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone,
                Image = model.Image
            };

            var created = await _staffRepository.CreateAsync(staff);
            model.StaffId = created.StaffId;
            return model;
        }

        public async Task UpdateAsync(StaffViewModel model)
        {
            var staff = await _staffRepository.GetByIdAsync(model.StaffId);
            if (staff == null) return;

            staff.FullName = model.FullName;
            staff.Gender = model.Gender;
            staff.DateOfBirth = model.DateOfBirth;
            staff.Address = model.Address;
            staff.Email = model.Email;
            staff.Phone = model.Phone;
            staff.Image = model.Image;

            await _staffRepository.UpdateAsync(staff);
        }

        public async Task DeleteAsync(int id)
        {
            await _staffRepository.DeleteAsync(id);
        }
    }
}
