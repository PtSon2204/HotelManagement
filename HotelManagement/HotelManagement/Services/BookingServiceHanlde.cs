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
                    Customer = x.Customer,
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

        public int NumberOfBookings()
        {
            return _bookingRepo.CountBooking();
        }

        public async Task<BookingViewModel> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepo.GetById(id);

            return new BookingViewModel
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId,
                Customer = booking.Customer,
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                Deposit = booking.Deposit,
                NumOfPeople = booking.NumOfPeople,
                CreatedDate = booking.CreatedDate,
                StaffId= booking.StaffId,
                Status = booking.Status,
                Room = booking.RoomBookings.FirstOrDefault()?.Room,
                Services = booking.BookingServices.Select(bs => bs.Service).ToList()
            };
        }

        public async Task BookingUpdateStatusAsync(int  id, string? status)
        {
            await _bookingRepo.BookingUpdateStatus(id, status);     
        }

        public async Task CheckInAsync(int id)
        {
            await _bookingRepo.CheckInAsync(id);
        }
        public async Task CheckOutAsync(int id, string paymentMethod)
        {
            await _bookingRepo.CheckOutAsync(id, paymentMethod);
        }

        public async Task CreateBookingDirectAsync(DirectBookingViewModel model)
        {
            await _bookingRepo.CreateDirectCheckInAsync(model);
        }
    }
}
