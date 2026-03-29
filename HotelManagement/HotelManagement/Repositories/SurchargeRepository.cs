using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagement.Repositories
{
    public class SurchargeRepository
    {
        private readonly ApplicationDbContext _context;

        public SurchargeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Surcharge>> GetAllAsync()
        {
            return await _context.Surcharges.Include(s => s.Invoice).ToListAsync();
        }

        public async Task<Surcharge?> GetByIdAsync(int id)
        {
            return await _context.Surcharges.Include(s => s.Invoice).FirstOrDefaultAsync(s => s.SurchargeId == id);
        }

        public async Task AddAsync(Surcharge surcharge)
        {
            // First, add the surcharge record
            _context.Surcharges.Add(surcharge);

            // Business Rule: Update the associated Invoice total
            var invoice = await _context.Invoices.FindAsync(surcharge.InvoiceId);
            if (invoice != null)
            {
                invoice.TotalAmount += surcharge.Amount;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Surcharge surcharge)
        {
            _context.Surcharges.Update(surcharge);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var surcharge = await _context.Surcharges.FindAsync(id);
            if (surcharge != null)
            {
                // Business Rule: Decrement the associated Invoice total before removing
                var invoice = await _context.Invoices.FindAsync(surcharge.InvoiceId);
                if (invoice != null)
                {
                    invoice.TotalAmount -= surcharge.Amount;
                }

                _context.Surcharges.Remove(surcharge);
                await _context.SaveChangesAsync();
            }
        }
    }
}
