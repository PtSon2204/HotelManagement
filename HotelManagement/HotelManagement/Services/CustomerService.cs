using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;
using Microsoft.EntityFrameworkCore;

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
                    CustomerId = x.CustomerId,
                    FullName = x.FullName,
                    Gender = x.Gender,
                    Idcard = x.Idcard,
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
            var customer = await _repo.GetCustomerById(id);

            if (customer == null)
            {
                throw new Exception($"{id} not exits");
            }

            return new CustomerViewModel
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Gender = customer.Gender,
                Idcard = customer.Idcard,
                Address = customer.Address,
                Nationality = customer.Nationality,
                Email = customer.Email,
                Phone = customer.Phone,
            };
        }
    }
}
