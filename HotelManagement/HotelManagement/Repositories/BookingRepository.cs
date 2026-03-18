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
                        .ThenInclude(rb => rb.Room) // Đi sâu vào RoomBooking để lấy ra Room
                        .ThenInclude(r => r.RoomType)
                        .FirstOrDefaultAsync(x => x.BookingId == id);
        }


        public async Task BookingUpdateStatus(int id, string? status)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                throw new Exception();
            }

            booking.Status = status.ToString();
            await _context.SaveChangesAsync();

        }

        public async Task CheckInAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
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

        public async Task CheckOutAsync(int bookingId, string paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(b => b.Rentals)
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            var rental = booking?.Rentals.FirstOrDefault(r => r.CheckOutActual == null);

            if (booking != null && booking.Status == "CheckedIn" && rental != null)
            {
                booking.Status = "CheckedOut";
                rental.CheckOutActual = DateTime.Now;

                var room = booking.RoomBookings.FirstOrDefault()?.Room;
                decimal roomPrice = room?.Price ?? 0;
                decimal deposit = booking.Deposit ?? 0;
                decimal totalAmount = roomPrice - deposit;

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
        public async Task ConfirmBooking(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                if (booking.Status == "Cancelled" || booking.Status == "CheckedOut" || booking.Status == "CheckedIn")
                {
                    return; 
                }

                booking.Status = status.ToString();
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateDirectCheckInAsync(DirectBookingViewModel model)
        {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == model.Phone || c.Idcard == model.IdCard);

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
                await _context.SaveChangesAsync();
            }

            var booking = new Booking
            {
                 CustomerId = customer.CustomerId,
                 CheckIn = model.CheckInDate,
                 CheckOut = model.CheckOutDate,
                 NumOfPeople = model.NumberOfPeople,
                 Status = "CheckedIn",
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

            var rental = new Rental
            {
                BookingId = booking.BookingId,
                CheckInActual = booking.CheckIn,
                CheckOutActual = booking.CheckOut,
                StaffId = model.StaffId,
            };

            _context.Rentals.Add(rental);

            var room = await _context.Rooms.FindAsync(model.RoomId);

            if (room != null)
            {
                room.Status = "Occupied";
            }

            await _context.SaveChangesAsync();
        }
    }
}
