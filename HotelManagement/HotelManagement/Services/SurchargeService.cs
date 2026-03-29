using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagement.Services
{
    public class SurchargeService
    {
        private readonly SurchargeRepository _repo;

        public SurchargeService(SurchargeRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<SurchargeViewModel>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(MapToViewModel).ToList();
        }

        public async Task<SurchargeViewModel?> GetByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item == null ? null : MapToViewModel(item);
        }

        public async Task CreateAsync(SurchargeViewModel model)
        {
            var entity = new Surcharge
            {
                InvoiceId = model.InvoiceId,
                Reason = model.Reason,
                Amount = model.Amount,
                CreatedDate = model.CreatedDate == default ? System.DateTime.Now : model.CreatedDate
            };
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(SurchargeViewModel model)
        {
            var entity = await _repo.GetByIdAsync(model.SurchargeId);
            if (entity != null)
            {
                entity.InvoiceId = model.InvoiceId;
                entity.Reason = model.Reason;
                entity.Amount = model.Amount;
                entity.CreatedDate = model.CreatedDate;
                await _repo.UpdateAsync(entity);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }

        private static SurchargeViewModel MapToViewModel(Surcharge s)
        {
            return new SurchargeViewModel
            {
                SurchargeId = s.SurchargeId,
                InvoiceId = s.InvoiceId,
                Reason = s.Reason,
                Amount = s.Amount,
                CreatedDate = s.CreatedDate,
                InvoiceNumber = s.Invoice?.InvoiceId.ToString() ?? "N/A"
            };
        }
    }
}
