using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class BookingServiceHandle
    {
        private readonly BookingRepository _repo;

        public BookingServiceHandle(BookingRepository repo)
        {
            _repo = repo;
        }

        public int NumberOfBookings() => _repo.NumberOfBookings();

        public async Task<PagedResult<BookingViewModel>> GetAllBookings(BookingStatus? status, int page, int pageSize)
            => await _repo.GetAllBookings(status, page, pageSize);

        public async Task<BookingViewModel?> GetBookingByIdAsync(int id)
            => await _repo.GetBookingById(id);

        public async Task BookingUpdateStatusAsync(int id, string? status)
            => await _repo.UpdateStatus(id, status);

        public async Task CheckInAsync(int id)
            => await _repo.CheckIn(id);

        public async Task CheckOutAsync(int id, string paymentMethod)
            => await _repo.CheckOut(id, paymentMethod);

        public async Task CreateBookingDirectAsync(DirectBookingViewModel model)
            => await _repo.CreateBookingDirect(model);
    }
}
