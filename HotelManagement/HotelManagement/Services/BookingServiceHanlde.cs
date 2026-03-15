using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class BookingServiceHanlde
    {
        private readonly BookingRepository _bookingRepo;

        public BookingServiceHanlde(BookingRepository bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<PagedResult<BookingViewModel>> GetAllBookings(BookingStatus? status, int page, int pageSize)
        {
            var result = await _bookingRepo.GetAllBookings(status, page, pageSize);

            return new PagedResult<BookingViewModel>
            {
                Items = result.Items.Select(x => new BookingViewModel
                {
                    BookingId = x.BookingId,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.FullName,
                    CheckIn = x.CheckIn,
                    CheckOut = x.CheckOut,
                    Deposit = x.Deposit,
                    NumOfPeople = x.NumOfPeople,
                    StaffId = x.StaffId,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate
                }).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
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
