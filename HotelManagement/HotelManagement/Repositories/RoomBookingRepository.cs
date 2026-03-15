using HotelManagement.Context;
using HotelManagement.Models.Entities;

namespace HotelManagement.Repositories
{
    public class RoomBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomBookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRange(List<RoomBooking> roomBookings)
        {
            _context.RoomBookings.AddRange(roomBookings);
            await _context.SaveChangesAsync();
        }
    }
}
