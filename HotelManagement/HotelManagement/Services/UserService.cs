using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class UserService
    {
        private readonly UserRepository _repo;

        public UserService(UserRepository repo)
        {
            _repo = repo;
        }

        public int CountCustomer() => _repo.CountCustomer();

        public async Task<PagedResult<CustomerViewModel>> GetCustomersAsync(string? search, int page, int pageSize)
            => await _repo.GetCustomersAsync(search, page, pageSize);

        public async Task<CustomerViewModel?> GetCustomerById(int id)
            => await _repo.GetCustomerById(id);
    }
}
