using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class InvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<InvoiceViewModel>> GetInvoicesAsync(string? search, int page, int pageSize)
        {
            var query = _context.Invoices.Select(i => new InvoiceViewModel
            {
                InvoiceId = i.InvoiceId,
                PaymentDate = i.PaymentDate,
                Status = i.Status,
                TotalAmount = i.TotalAmount,

                // Map nhánh Khách hàng
                CustomerName = i.Rental.Booking.Customer.FullName,
                CustomerPhone = i.Rental.Booking.Customer.Phone,
                Email = i.Rental.Booking.Customer.Email,

                // Map nhánh nhân viên
                StaffName = i.Rental.Booking.Staff.FullName,

                // Map nhánh Booking
                BookingId = i.Rental.BookingId,
                CheckIn = i.Rental.Booking.CheckIn,
                CheckOut = i.Rental.Booking.CheckOut,
                Deposit = i.Rental.Booking.Deposit,

                // Map nhánh Phòng 
                RoomNumber = i.Rental.Booking.RoomBookings.FirstOrDefault().Room.RoomNumber,
                RoomTypeName = i.Rental.Booking.RoomBookings.FirstOrDefault().Room.RoomType.Name,
                RoomPrice = i.Rental.Booking.RoomBookings.FirstOrDefault().Room.Price,

                // Map nhánh Dịch vụ
                Services = i.Rental.Booking.BookingServices.Select(bs => new InvoiceServiceItem
                {
                    ServiceName = bs.Service.Name,
                    Price = bs.Service.Price
                }).ToList()
            });

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.RoomNumber != null && x.RoomNumber.Contains(search)) ||
                    (x.CustomerName != null && x.CustomerName.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(x => x.InvoiceId)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .AsNoTracking()
                                   .ToListAsync();

            return new PagedResult<InvoiceViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<InvoiceViewModel?> GetInvoiceByIdAsync(int id)
        {
            // BƯỚC 1: Tìm ID và Select trực tiếp (Không dùng Include, không gọi FirstOrDefaultAsync vội)
            var invoice = await _context.Invoices
                .Where(x => x.InvoiceId == id)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    PaymentDate = i.PaymentDate,
                    Status = i.Status,
                    TotalAmount = i.TotalAmount,

                    // Map nhánh Khách hàng
                    CustomerName = i.Rental.Booking.Customer.FullName,
                    CustomerPhone = i.Rental.Booking.Customer.Phone,
                    Email = i.Rental.Booking.Customer.Email,
                    IdCard = i.Rental.Booking.Customer.Idcard, // Chữ "Idcard" viết đúng theo Entity của bạn
                    Address = i.Rental.Booking.Customer.Address,

                    // Map nhánh nhân viên
                    StaffName = i.Rental.Booking.Staff.FullName,

                    // Map nhánh Booking
                    BookingId = i.Rental.BookingId,
                    CheckIn = i.Rental.Booking.CheckIn,
                    CheckOut = i.Rental.Booking.CheckOut,
                    Deposit = i.Rental.Booking.Deposit,

                    // Map nhánh Phòng 
                    RoomNumber = i.Rental.Booking.RoomBookings.FirstOrDefault().Room.RoomNumber,
                    RoomTypeName = i.Rental.Booking.RoomBookings.FirstOrDefault().Room.RoomType.Name,
                    RoomPrice = i.Rental.Booking.RoomBookings.FirstOrDefault().Room.Price,

                    // Map nhánh Dịch vụ
                    Services = i.Rental.Booking.BookingServices.Select(bs => new InvoiceServiceItem
                    {
                        ServiceName = bs.Service.Name,
                        Price = bs.Service.Price
                    }).ToList()

                }) // Đóng ngoặc Select
                .FirstOrDefaultAsync(); // BƯỚC 2: GỌI LẤY DỮ LIỆU Ở DÒNG CUỐI CÙNG NÀY

            return invoice;
        }
    }
}
