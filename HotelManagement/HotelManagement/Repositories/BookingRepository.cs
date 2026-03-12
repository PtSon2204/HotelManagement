using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class BookingRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BookingRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<Booking>> GetPendingBookings()
        {
            return await _applicationDbContext.Bookings.Where(b => b.Status == "Pending").ToListAsync();
        }

        public async Task<Booking?> GetById(int id)
        {
            return await _applicationDbContext.Bookings.FindAsync(id);
        }

        public async Task ConfirmBooking(int id, BookingStatus status)
        {
            var booking = await _applicationDbContext.Bookings.FindAsync(id);

            booking.Status = status.ToString();
            await _applicationDbContext.SaveChangesAsync();
        }

        public int CountBooking()
        {
            return _applicationDbContext.Bookings.Count();
        }
    }
}
