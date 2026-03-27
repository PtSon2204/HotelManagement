using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _repo;

        public CustomerService(CustomerRepository repo)
        {
            _repo = repo;
        }

        public int CountCustomer() => _repo.CountCustomer();

        public async Task<PagedResult<CustomerViewModel>> GetCustomersAsync(string? search, int page, int pageSize)
        {
            var result = await _repo.GetCustomersAsync(search, page, pageSize);

            return new PagedResult<CustomerViewModel>
            {
                Items = result.Items.Select(x => new CustomerViewModel
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Gender = x.Gender,
                    IDCard = x.IDCard,
                    Address = x.Address,
                    Nationality = x.Nationality,
                    Email = x.Email,
                    Phone = x.Phone
                }).ToList(),

                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<CustomerViewModel> GetCustomerById(int id)
        {
            var user = await _repo.GetCustomerById(id);

            if (user == null)
            {
                throw new Exception($"{id} not found");
            }

            return new CustomerViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Gender = user.Gender,
                IDCard = user.IDCard,
                Address = user.Address,
                Nationality = user.Nationality,
                Email = user.Email,
                Phone = user.Phone,
            };
        }
    }
}
