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
                query = query.Where(x => x.Status != null && x.Status.Contains(status.ToString()));
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
                        .Include(x => x.BookingServices).ThenInclude(bs => bs.Service)
                        .FirstOrDefaultAsync(x => x.BookingId == id);
        }

        public async Task BookingUpdateStatus(int id, string? status)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            var room = booking.RoomBookings.FirstOrDefault()?.Room;
            booking.Status = status;

            if (booking.Status == "Confirmed" && room != null)
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
                booking.ActualCheckIn = DateTime.Now;

                var room = booking.RoomBookings.FirstOrDefault()?.Room;
                if (room != null)
                {
                    room.Status = "Occupied";
                }

                await _context.SaveChangesAsync();
            }
        }

        private int NumberOfDay(DateTime? checkOut, DateTime? checkIn)
        {
            int days = (checkOut!.Value.Date - checkIn!.Value.Date).Days;
            return days > 0 ? days : 1;
        }

        public async Task CheckOutAsync(int bookingId, string paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking != null && booking.Status == "CheckedIn")
            {
                booking.Status = "CheckedOut";
                booking.ActualCheckOut = DateTime.Now;

                var room = booking.RoomBookings.FirstOrDefault()?.Room;
                decimal roomPrice = (room?.Price ?? 0) * NumberOfDay(booking.ActualCheckOut, booking.ActualCheckIn);
                decimal deposit = booking.Deposit ?? 0;
                decimal serviceTotal = booking.BookingServices.Sum(bs => bs.Service?.Price ?? 0);
                decimal totalAmount = roomPrice + serviceTotal - deposit;

                var invoice = new Invoice
                {
                    BookingId = bookingId,
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

        // Check-in trực tiếp khi khách đến tại quầy
        public async Task<int> CreateDirectCheckInAsync(DirectBookingViewModel model)
        {
            var booking = new Booking
            {
                UserId = model.UserId ?? 0,
                ExpectedCheckIn = model.CheckInDate,
                ExpectedCheckOut = model.CheckOutDate,
                NumOfPeople = model.NumberOfPeople,
                Status = model.StaffId.HasValue ? "CheckedIn" : "Pending",
                CreatedDate = DateTime.Now,
                StaffId = model.StaffId,
            };

            // Nếu chưa có UserId (khách vãng lai), tìm hoặc tạo User tạm
            if (booking.UserId == 0)
            {
                // Tìm role Customer
                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
                if (customerRole == null) throw new Exception("Không tìm thấy Role 'Customer'");

                // Tìm user theo số điện thoại hoặc IDCard
                User? existingUser = null;
                if (!string.IsNullOrWhiteSpace(model.IdCard))
                    existingUser = await _context.Users.FirstOrDefaultAsync(u => u.IDCard == model.IdCard);
                if (existingUser == null && !string.IsNullOrWhiteSpace(model.Phone))
                    existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == model.Phone);

                if (existingUser != null)
                {
                    booking.UserId = existingUser.UserId;
                    // Cập nhật thông tin nếu cần
                    if (!string.IsNullOrWhiteSpace(model.FullName)) existingUser.FullName = model.FullName;
                    if (!string.IsNullOrWhiteSpace(model.Email)) existingUser.Email = model.Email;
                    if (!string.IsNullOrWhiteSpace(model.Address)) existingUser.Address = model.Address;
                    if (!string.IsNullOrWhiteSpace(model.Gender)) existingUser.Gender = model.Gender;
                    if (!string.IsNullOrWhiteSpace(model.Nationality)) existingUser.Nationality = model.Nationality;
                    _context.Users.Update(existingUser);
                }
                else
                {
                    // Tạo user mới cho khách vãng lai
                    var guestUser = new User
                    {
                        RoleId = customerRole.RoleId,
                        Username = "guest_" + (model.Phone ?? Guid.NewGuid().ToString("N")[..8]),
                        PasswordHash = string.Empty,
                        FullName = model.FullName,
                        Email = model.Email,
                        IDCard = model.IdCard,
                        Phone = model.Phone,
                        Address = model.Address,
                        Gender = model.Gender,
                        Nationality = model.Nationality
                    };
                    _context.Users.Add(guestUser);
                    await _context.SaveChangesAsync();
                    booking.UserId = guestUser.UserId;
                }
            }

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
                    _context.BookingServices.Add(new BookingService
                    {
                        BookingId = booking.BookingId,
                        ServiceId = sId
                    });
                }
            }

            if (model.StaffId.HasValue)
            {
                booking.ActualCheckIn = booking.ExpectedCheckIn;
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
                             rb.Booking.ExpectedCheckIn < checkOut &&
                             rb.Booking.ExpectedCheckOut > checkIn)
                .AnyAsync();

            return !overlappingBookings;
        }
    }
}
