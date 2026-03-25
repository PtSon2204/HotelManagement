using System.Linq.Expressions;
using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
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
            return await _context.Bookings
                        .Include(x => x.Customer)
                        .Include(x => x.RoomBookings)
                        .ThenInclude(rb => rb.Room) 
                        .ThenInclude(r => r.RoomType)
                        .Include(x => x.BookingServices).ThenInclude(bs => bs.Service)
                        .FirstOrDefaultAsync(x => x.BookingId == id);
        }


        public async Task BookingUpdateStatus(int id, string? status)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            var room = booking.RoomBookings.FirstOrDefault()?.Room;
            if (booking == null)
            {
                throw new Exception();
            }

            booking.Status = status.ToString();
            if (booking.Status == "Confirmed")
            {
                room.Status = "Occupied";
            }
            await _context.SaveChangesAsync();

        }

        public async Task CheckInAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .Include(s => s.BookingServices).ThenInclude(sb => sb.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking != null && (booking.Status == "Pending" || booking.Status == "Confirmed"))
            {
                booking.Status = "CheckedIn";
                var rental = new Rental
                {
                    BookingId = bookingId,
                    CheckInActual = DateTime.Now
                };
                _context.Rentals.Add(rental);

                var room = booking.RoomBookings.FirstOrDefault()?.Room;
                if (room != null)
                {
                    room.Status = "Occupied";
                }

                await _context.SaveChangesAsync();
            }
        }

        private int NumberOfDay(DateTime? CheckOut, DateTime? CheckIn)
        {
            int days = (CheckOut.Value.Date - CheckIn.Value.Date).Days;
            return days > 0 ? days : 1;
        }
        public async Task CheckOutAsync(int bookingId, string paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(b => b.Rentals)
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            var rental = booking?.Rentals.FirstOrDefault(r => r.CheckOutActual == null);

            if (booking != null && booking.Status == "CheckedIn" && rental != null)
            {
                booking.Status = "CheckedOut";
                rental.CheckOutActual = DateTime.Now;

                var room = booking.RoomBookings.FirstOrDefault()?.Room;
                decimal roomPrice = (room?.Price ?? 0) * NumberOfDay(rental.CheckOutActual, rental.CheckInActual);
                decimal deposit = booking.Deposit ?? 0;
                decimal serviceTotal = booking.BookingServices.Sum(bs => bs.Service?.Price ?? 0);
                decimal totalAmount = roomPrice + serviceTotal - deposit;

                var invoice = new Invoice
                {
                    RentalId = rental.RentalId,
                    TotalAmount = totalAmount > 0 ? totalAmount : 0,
                    PaymentDate = DateTime.Now,
                    Status = "Đã thanh toán - " + paymentMethod
                };
                _context.Invoices.Add(invoice);

                if (room != null)
                {
                    room.Status = "Available";
                }

                await _context.SaveChangesAsync();
            }
        }

        //check-in trực tiếp khi khách đến tại quầy hoặc đặt phòng trực tuyến
        public async Task<int> CreateDirectCheckInAsync(DirectBookingViewModel model)
        {
            Customer? customer = null;

            if (model.CustomerId.HasValue)
            {
                customer = await _context.Customers.FindAsync(model.CustomerId.Value);
            }

            if (customer == null && !string.IsNullOrWhiteSpace(model.IdCard))
            {
                customer = await _context.Customers.FirstOrDefaultAsync(c => c.Idcard == model.IdCard);
            }

            if (customer == null && !string.IsNullOrWhiteSpace(model.Phone))
            {
                customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == model.Phone);
            }

            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Idcard = model.IdCard,
                    Phone = model.Phone,
                    Address = model.Address,
                    Gender = model.Gender,
                    Nationality = model.Nationality
                };
                _context.Customers.Add(customer);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(model.FullName)) customer.FullName = model.FullName;
                if (!string.IsNullOrWhiteSpace(model.Email)) customer.Email = model.Email;
                if (!string.IsNullOrWhiteSpace(model.IdCard)) customer.Idcard = model.IdCard;
                if (!string.IsNullOrWhiteSpace(model.Phone)) customer.Phone = model.Phone;
                if (!string.IsNullOrWhiteSpace(model.Address)) customer.Address = model.Address;
                if (!string.IsNullOrWhiteSpace(model.Gender)) customer.Gender = model.Gender;
                if (!string.IsNullOrWhiteSpace(model.Nationality)) customer.Nationality = model.Nationality;
                
                _context.Customers.Update(customer);
            }
            await _context.SaveChangesAsync();

            var booking = new Booking
            {
                 CustomerId = customer.CustomerId,
                 CheckIn = model.CheckInDate,
                 CheckOut = model.CheckOutDate,
                 NumOfPeople = model.NumberOfPeople,
                 Status = model.StaffId.HasValue ? "CheckedIn" : "Pending",
                 CreatedDate = DateTime.Now,
                 StaffId = model.StaffId,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var roomBooking = new RoomBooking
            {
                BookingId = booking.BookingId,
                RoomId = model.RoomId
            };

            _context.RoomBookings.Add(roomBooking);

            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                foreach (var sId in model.SelectedServiceIds)
                {
                    var bookingService = new BookingService
                    {
                        BookingId = booking.BookingId,
                        ServiceId = sId
                    };
                    _context.BookingServices.Add(bookingService);
                }
            }

            if (model.StaffId.HasValue)
            {
                var rental = new Rental
                {
                    BookingId = booking.BookingId,
                    CheckInActual = booking.CheckIn,
                    StaffId = model.StaffId,
                };

                _context.Rentals.Add(rental);
            }

            var room = await _context.Rooms.FindAsync(model.RoomId);

            if (room != null)
            {
                room.Status = "Occupied";
            }

            await _context.SaveChangesAsync();
            return booking.BookingId;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var overlappingBookings = await _context.RoomBookings
                .Include(rb => rb.Booking)
                .Where(rb => rb.RoomId == roomId &&
                             rb.Booking != null &&
                             rb.Booking.Status != "Cancelled" &&
                             rb.Booking.Status != "Rejected" &&
                             rb.Booking.CheckIn < checkOut &&
                             rb.Booking.CheckOut > checkIn)
                .AnyAsync();

            return !overlappingBookings;
        }
    }
}
