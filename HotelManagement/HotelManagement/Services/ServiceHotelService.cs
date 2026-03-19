using HotelManagement.Models.Entities;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class ServiceHotelService
    {
        private readonly ServiceRepository _repo;

        public ServiceHotelService(ServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Service>> GetServicesAsync() => await _repo.GetServicesAsync();
    }
}
