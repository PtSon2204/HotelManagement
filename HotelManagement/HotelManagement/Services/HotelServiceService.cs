using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class HotelServiceService
    {
        private readonly ServiceRepository _serviceRepository;

        public HotelServiceService(ServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<List<ServiceViewModel>> GetAllAsync()
        {
            var services = await _serviceRepository.GetAllAsync();
            return services.Select(s => new ServiceViewModel
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Price = s.Price,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<ServiceViewModel?> GetByIdAsync(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null) return null;

            return new ServiceViewModel
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price,
                IsActive = service.IsActive
            };
        }

        public async Task<ServiceViewModel> CreateAsync(ServiceViewModel model)
        {
            var service = new Service
            {
                Name = model.Name,
                Price = model.Price,
                IsActive = model.IsActive ?? true
            };

            var created = await _serviceRepository.CreateAsync(service);
            return new ServiceViewModel
            {
                ServiceId = created.ServiceId,
                Name = created.Name,
                Price = created.Price,
                IsActive = created.IsActive
            };
        }

        public async Task UpdateAsync(ServiceViewModel model)
        {
            var service = await _serviceRepository.GetByIdAsync(model.ServiceId);
            if (service == null) return;

            service.Name = model.Name;
            service.Price = model.Price;
            service.IsActive = model.IsActive;

            await _serviceRepository.UpdateAsync(service);
        }

        public async Task DeleteAsync(int id)
        {
            await _serviceRepository.DeleteAsync(id);
        }
    }
}
