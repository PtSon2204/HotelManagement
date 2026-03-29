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

        public int CountService() => _repo.CountService();

        public async Task<List<Service>> GetAllAsync()
            => await _repo.GetAllAsync();
    }
}
