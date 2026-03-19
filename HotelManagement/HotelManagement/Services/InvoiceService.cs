using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _repo;

        public InvoiceService(InvoiceRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<InvoiceViewModel>> GetAllInvoicesAsync(string? search, int page, int pageSize)
        {
            return await _repo.GetInvoicesAsync(search, page, pageSize);
        }

        public async Task<InvoiceViewModel> GetInvoiceByIdAsync(int id)
        {
            return await _repo.GetInvoiceByIdAsync(id);
        }
    }
}
