using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class BookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public int CountBooking()
        {
            return _context.Bookings.Count();
        }

        public async Task<PagedResult<Booking>> GetAllBookings(BookingStatus? status, int page, int pageSize)
        {
            var query = _context.Bookings
                .Include(x => x.Customer)
                .AsQueryable();

            if (status != null)
            {
                query = query.Where(x => x.Status.Contains(status.ToString()));
            }

            int totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(x => x.BookingId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Booking>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Booking?> GetById(int id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task ConfirmBooking(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);

            booking.Status = status.ToString();
            await _context.SaveChangesAsync();
        }
    }
}
