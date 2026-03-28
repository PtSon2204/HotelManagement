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

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public int NumberOfBookings()
        {
            return _context.Bookings.Count();
        }
        public async Task<PagedResult<BookingViewModel>> GetAllBookings(BookingStatus? status, int page, int pageSize)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .AsQueryable();

            if (status.HasValue)
            {
                string statusStr = status.Value.ToString();
                query = query.Where(b => b.Status == statusStr);
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    UserId = b.UserId,
                    ExpectedCheckIn = b.ExpectedCheckIn,
                    ExpectedCheckOut = b.ExpectedCheckOut,
                    ActualCheckIn = b.ActualCheckIn,
                    ActualCheckOut = b.ActualCheckOut,
                    Deposit = b.Deposit,
                    NumOfPeople = b.NumOfPeople,
                    Status = b.Status,
                    CreatedDate = b.CreatedDate,
                    Room = b.Room,
                    Customer = b.User,
                    Services = b.BookingServices
                        .Where(bs => bs.Service != null)
                        .Select(bs => bs.Service!)
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<BookingViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<BookingViewModel?> GetBookingById(int id)
        {
            var b = await _context.Bookings
                .Include(x => x.User)
                .Include(x => x.Room)
                .Include(x => x.BookingServices).ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(x => x.BookingId == id);

            if (b == null) return null;

            return new BookingViewModel
            {
                BookingId = b.BookingId,
                UserId = b.UserId,
                ExpectedCheckIn = b.ExpectedCheckIn,
                ExpectedCheckOut = b.ExpectedCheckOut,
                ActualCheckIn = b.ActualCheckIn,
                ActualCheckOut = b.ActualCheckOut,
                Deposit = b.Deposit,
                NumOfPeople = b.NumOfPeople,
                Status = b.Status,
                CreatedDate = b.CreatedDate,
                Room = b.Room,
                Customer = b.User,
                Services = b.BookingServices
                    .Where(bs => bs.Service != null)
                    .Select(bs => bs.Service!)
                    .ToList()
            };
        }

        public async Task UpdateStatus(int id, string? status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return;

            booking.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task CheckIn(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return;

            booking.ActualCheckIn = DateTime.Now;
            booking.Status = "CheckedIn";

            if (booking.Room != null)
                booking.Room.Status = "Occupied";

            await _context.SaveChangesAsync();
        }

        public async Task CheckOut(int id, string paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return;

            booking.ActualCheckOut = DateTime.Now;
            booking.Status = "CheckedOut";

            if (booking.Room != null)
                booking.Room.Status = "Available";

            // Tính tổng tiền
            int days = (booking.ExpectedCheckOut.Date - booking.ExpectedCheckIn.Date).Days;
            int numberOfDays = days > 0 ? days : 1;
            decimal roomPrice = (booking.Room?.Price ?? 0) * numberOfDays;
            decimal serviceTotal = booking.BookingServices
                .Where(bs => bs.Service != null)
                .Sum(bs => bs.Service!.Price);
            decimal deposit = booking.Deposit ?? 0;
            decimal totalAmount = roomPrice + serviceTotal - deposit;

            // Tạo Invoice nếu chưa có
            if (booking.Invoice == null)
            {
                var invoice = new Invoice
                {
                    BookingId = booking.BookingId,
                    TotalAmount = totalAmount > 0 ? totalAmount : 0,
                    PaymentDate = DateTime.Now,
                    Status = "Đã thanh toán"
                };
                _context.Invoices.Add(invoice);
            }
            else
            {
                booking.Invoice.TotalAmount = totalAmount > 0 ? totalAmount : 0;
                booking.Invoice.PaymentDate = DateTime.Now;
                booking.Invoice.Status = "Đã thanh toán";
            }

            await _context.SaveChangesAsync();
        }

        // ──────────────── CUSTOMER HISTORY ─────────────
        public async Task<CustomerHistoryViewModel?> GetCustomerHistoryAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .Include(b => b.Invoice)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            var completedBookings = bookings.Where(b => b.Status == "CheckedOut").ToList();

            int totalNights = completedBookings.Sum(b =>
            {
                var co = b.ActualCheckOut ?? b.ExpectedCheckOut;
                var ci = b.ActualCheckIn  ?? b.ExpectedCheckIn;
                int d  = (co.Date - ci.Date).Days;
                return d > 0 ? d : 1;
            });

            decimal totalSpent = completedBookings
                .Where(b => b.Invoice != null)
                .Sum(b => b.Invoice!.TotalAmount);

            string? favRoomType = completedBookings
                .Where(b => b.Room?.RoomTypeName != null)
                .GroupBy(b => b.Room!.RoomTypeName)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var topServices = bookings
                .SelectMany(b => b.BookingServices)
                .Where(bs => bs.Service != null)
                .GroupBy(bs => bs.Service!.Name)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new ServiceUsageStat
                {
                    ServiceName = g.Key ?? "(Không tên)",
                    UsageCount  = g.Count()
                })
                .ToList();

            var stayHistory = bookings.Select(b => new StayRecord
            {
                BookingId        = b.BookingId,
                RoomNumber       = b.Room?.RoomNumber,
                RoomTypeName     = b.Room?.RoomTypeName,
                RoomPrice        = b.Room?.Price,
                NumOfPeople      = b.NumOfPeople,
                Status           = b.Status,
                ExpectedCheckIn  = b.ExpectedCheckIn,
                ExpectedCheckOut = b.ExpectedCheckOut,
                ActualCheckIn    = b.ActualCheckIn,
                ActualCheckOut   = b.ActualCheckOut,
                TotalAmount      = b.Invoice?.TotalAmount,
                Services         = b.BookingServices
                    .Where(bs => bs.Service != null)
                    .Select(bs => bs.Service!.Name)
                    .ToList()
            }).ToList();

            return new CustomerHistoryViewModel
            {
                UserId              = user.UserId,
                FullName            = user.FullName ?? "(Chưa có tên)",
                Phone               = user.Phone,
                Email               = user.Email,
                IDCard              = user.IDCard,
                Gender              = user.Gender,
                Nationality         = user.Nationality,
                Address             = user.Address,
                TotalCompletedStays = completedBookings.Count,
                TotalNights         = totalNights,
                TotalSpent          = totalSpent,
                FavoriteRoomType    = favRoomType,
                TopServices         = topServices,
                StayHistory         = stayHistory
            };
        }

        // ──────────────── DIRECT BOOKING ────────────────
        public async Task CreateBookingDirect(DirectBookingViewModel model)
        {
            // Tìm role Customer
            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Customer");

            if (customerRole == null)
                throw new Exception("Không tìm thấy role Customer trong hệ thống.");

            // Tìm hoặc tạo Customer theo IDCard hoặc Phone
            User? customer = null;
            if (!string.IsNullOrWhiteSpace(model.IdCard))
                customer = await _context.Users
                    .FirstOrDefaultAsync(u => u.IDCard == model.IdCard);

            if (customer == null && !string.IsNullOrWhiteSpace(model.Phone))
                customer = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == model.Phone && u.RoleId == customerRole.RoleId);

            if (customer == null)
            {
                customer = new User
                {
                    RoleId       = customerRole.RoleId,
                    Username     = "guest_" + Guid.NewGuid().ToString("N")[..8],
                    PasswordHash = string.Empty,
                    FullName     = model.FullName,
                    IDCard       = model.IdCard,
                    Phone        = model.Phone,
                    Email        = model.Email,
                    Address      = model.Address,
                    Gender       = model.Gender,
                    Nationality  = model.Nationality
                };
                _context.Users.Add(customer);
                await _context.SaveChangesAsync();
            }

            // ── Tính tiền phòng theo số đêm ──────────────────────────
            var room = await _context.Rooms.FindAsync(model.RoomId);
            int nights = (model.CheckOutDate.Date - model.CheckInDate.Date).Days;
            if (nights <= 0) nights = 1;
            decimal roomTotal = (room?.Price ?? 0) * nights;

            // ── Tính tiền dịch vụ ────────────────────────────────────
            decimal serviceTotal = 0;
            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                var svcs = await _context.Services
                    .Where(s => model.SelectedServiceIds.Contains(s.ServiceId))
                    .ToListAsync();
                serviceTotal = svcs.Sum(s => s.Price);
            }

            decimal grandTotal    = roomTotal + serviceTotal;
            decimal depositAmount = 0;  // Booking trực tiếp tại quầy: không cần đặt cọc

            // ── Tạo Booking ──────────────────────────────────────────
            var booking = new Booking
            {
                UserId           = customer.UserId,
                RoomId           = model.RoomId,
                ExpectedCheckIn  = model.CheckInDate,
                ExpectedCheckOut = model.CheckOutDate,
                NumOfPeople      = model.NumberOfPeople,
                Deposit          = depositAmount,
                Status           = "Confirmed",
                CreatedDate      = DateTime.Now
            };
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // ── Cập nhật trạng thái phòng ────────────────────────────
            if (room != null)
            {
                room.Status = "Occupied";   // ← sửa lỗi 'Occupid'
                await _context.SaveChangesAsync();
            }

            // ── Gắn dịch vụ đã chọn ─────────────────────────────────
            if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
            {
                foreach (var serviceId in model.SelectedServiceIds)
                {
                    _context.BookingServices.Add(new BookingService
                    {
                        BookingId = booking.BookingId,
                        ServiceId = serviceId
                    });
                }
                await _context.SaveChangesAsync();
            }

            // ── Tạo Invoice ──────────────────────────────────────────
            var invoice = new Invoice
            {
                BookingId   = booking.BookingId,
                TotalAmount = grandTotal,
                Status      = "Chưa thanh toán",
                PaymentDate = null
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }
    }
}
