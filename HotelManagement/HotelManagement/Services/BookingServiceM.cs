using HotelManagement.Models.Entities;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class BookingServiceM
    {
        private readonly BookingRepository _bookingRepo;

        public BookingServiceM(BookingRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<List<Booking>> GetPedingBookings()
        {
            return await _bookingRepo.GetPendingBookings();
        }

        public async Task ConfirmBooking(int id, BookingStatus status)
        {
            await _bookingRepo.ConfirmBooking(id, status);
        }

        public int NumberOfBookings()
        {
            return _bookingRepo.CountBooking();
        }
    }
}
