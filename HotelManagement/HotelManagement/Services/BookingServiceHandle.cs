using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;
using HotelManagement.Context;

using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class BookingServiceHandle
    {
        private readonly BookingRepository _repo;
        private readonly ApplicationDbContext _context; 

        public BookingServiceHandle(BookingRepository repo, ApplicationDbContext context)
        {
            _repo = repo;
            _context = context;
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

        public async Task AddServicesToBookingAsync(int bookingId, List<int> serviceIds)
            => await _repo.AddServicesToBooking(bookingId, serviceIds);

        public async Task CreateBookingDirectAsync(DirectBookingViewModel model)
            => await _repo.CreateBookingDirect(model);

        public async Task<CustomerHistoryViewModel?> GetCustomerHistoryAsync(int userId)
            => await _repo.GetCustomerHistoryAsync(userId);


        /// <summary>Kiểm tra xem phòng có available trong khoảng thời gian không.</summary>
        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var conflict = await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                b.Status != "Đã hủy" &&
                b.Status != "Cancelled" &&
                b.ExpectedCheckIn < checkOut &&
                b.ExpectedCheckOut > checkIn);

            return !conflict;
        }

        /// <summary>Tạo booking mới. Trả về BookingId.</summary>
        public async Task<int> CreateBookingAsync(DirectBookingViewModel model, int userId)
        {
            // Tính toán tiền phòng
            var room = await _context.Rooms.FindAsync(model.RoomId)
                ?? throw new Exception("Phòng không tồn tại.");

            int days = (model.CheckOutDate.Date - model.CheckInDate.Date).Days;
            if (days <= 0) days = 1;

            decimal roomTotal = room.Price * days;
            decimal servicesTotal = 0;

            if (model.SelectedServiceIds.Any())
            {
                var services = await _context.Services
                    .Where(s => model.SelectedServiceIds.Contains(s.ServiceId))
                    .ToListAsync();
                servicesTotal = services.Sum(s => s.Price);
            }

            decimal grandTotal = roomTotal + servicesTotal;

            // Tính discount và deposit theo phương thức thanh toán
            decimal discountRate = 0;
            decimal depositAmount = 0;

            if (model.PaymentOption == "Full")
            {
                discountRate = 0.07m; // 7% discount khi thanh toán đủ
                grandTotal = grandTotal * (1 - discountRate);
                depositAmount = grandTotal; // 100%
            }
            else // Deposit 50%
            {
                depositAmount = grandTotal * 0.5m;
            }

            // Determine GuestProfileId
            int? finalGuestProfileId = model.GuestProfileId;

            // Chỉ tạo GuestProfile mới khi người dùng bật "Lưu thông tin khách" VÀ chưa chọn profile có sẵn.
            // KHÔNG cập nhật bảng Users — thông tin người đặt hộ chỉ lưu vào GuestProfile,
            // không được ghi đè thông tin cá nhân của chủ tài khoản.
            if (model.SaveProfile && (model.GuestProfileId == null || model.GuestProfileId == 0))
            {
                var newProfile = new GuestProfile
                {
                    UserId      = userId,
                    Label       = string.IsNullOrWhiteSpace(model.GuestLabel) ? "Khách mượn" : model.GuestLabel,
                    FullName    = model.FullName,
                    Phone       = model.Phone,
                    Email       = model.Email,
                    IdCard      = model.IdCard,
                    Gender      = model.Gender,
                    Nationality = model.Nationality,
                    Address     = model.Address
                };
                _context.GuestProfiles.Add(newProfile);
                await _context.SaveChangesAsync();
                finalGuestProfileId = newProfile.ProfileId;
            }

            // Tạo booking
            var booking = new Booking
            {
                UserId           = userId,
                RoomId           = model.RoomId,
                ExpectedCheckIn  = model.CheckInDate,
                ExpectedCheckOut = model.CheckOutDate,
                NumOfPeople      = model.NumberOfPeople,
                Deposit          = depositAmount,
                Status           = "Chờ xác nhận",
                CreatedDate      = DateTime.Now,
                GuestProfileId   = finalGuestProfileId
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(); // Lấy BookingId

            // Thêm dịch vụ kèm theo
            if (model.SelectedServiceIds.Any())
            {
                foreach (var svcId in model.SelectedServiceIds)
                {
                    _context.BookingServices.Add(new BookingService
                    {
                        BookingId = booking.BookingId,
                        ServiceId = svcId
                    });
                }
            }

            // Tạo Invoice
            var invoice = new Invoice
            {
                BookingId = booking.BookingId,
                TotalAmount = grandTotal,
                Status = "Chưa thanh toán",
                PaymentDate = null
            };
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return booking.BookingId;
        }
    }
}
