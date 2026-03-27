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

                // Map thông tin Khách hàng qua Booking.Customer
                CustomerName = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.FullName : null,
                CustomerPhone = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.Phone : null,
                Email = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.Email : null,

                // Map thông tin Nhân viên
                StaffName = i.Booking != null && i.Booking.Staff != null ? i.Booking.Staff.FullName : null,

                // Map thông tin Booking
                BookingId = i.BookingId,
                CheckIn = i.Booking != null ? i.Booking.ExpectedCheckIn : default,
                CheckOut = i.Booking != null ? i.Booking.ExpectedCheckOut : default,
                Deposit = i.Booking != null ? i.Booking.Deposit : null,

                // Map thông tin Phòng
                RoomNumber = i.Booking != null
                    ? i.Booking.RoomBookings.Select(rb => rb.Room!.RoomNumber).FirstOrDefault()
                    : null,
                RoomTypeName = i.Booking != null
                    ? i.Booking.RoomBookings.Select(rb => rb.Room!.RoomTypeName).FirstOrDefault()
                    : null,
                RoomPrice = i.Booking != null
                    ? i.Booking.RoomBookings.Select(rb => (decimal?)rb.Room!.Price).FirstOrDefault()
                    : null,

                // Map dịch vụ
                Services = i.Booking != null
                    ? i.Booking.BookingServices.Select(bs => new InvoiceServiceItem
                    {
                        ServiceName = bs.Service!.Name,
                        Price = bs.Service.Price
                    }).ToList()
                    : new List<InvoiceServiceItem>()
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
            var invoice = await _context.Invoices
                .Where(x => x.InvoiceId == id)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    PaymentDate = i.PaymentDate,
                    Status = i.Status,
                    TotalAmount = i.TotalAmount,

                    // Khách hàng
                    CustomerName = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.FullName : null,
                    CustomerPhone = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.Phone : null,
                    Email = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.Email : null,
                    IdCard = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.IDCard : null,
                    Address = i.Booking != null && i.Booking.Customer != null ? i.Booking.Customer.Address : null,

                    // Nhân viên
                    StaffName = i.Booking != null && i.Booking.Staff != null ? i.Booking.Staff.FullName : null,

                    // Booking
                    BookingId = i.BookingId,
                    CheckIn = i.Booking != null ? i.Booking.ExpectedCheckIn : default,
                    CheckOut = i.Booking != null ? i.Booking.ExpectedCheckOut : default,
                    Deposit = i.Booking != null ? i.Booking.Deposit : null,

                    // Phòng
                    RoomNumber = i.Booking != null
                        ? i.Booking.RoomBookings.Select(rb => rb.Room!.RoomNumber).FirstOrDefault()
                        : null,
                    RoomTypeName = i.Booking != null
                        ? i.Booking.RoomBookings.Select(rb => rb.Room!.RoomTypeName).FirstOrDefault()
                        : null,
                    RoomPrice = i.Booking != null
                        ? i.Booking.RoomBookings.Select(rb => (decimal?)rb.Room!.Price).FirstOrDefault()
                        : null,

                    // Dịch vụ
                    Services = i.Booking != null
                        ? i.Booking.BookingServices.Select(bs => new InvoiceServiceItem
                        {
                            ServiceName = bs.Service!.Name,
                            Price = bs.Service.Price
                        }).ToList()
                        : new List<InvoiceServiceItem>()
                })
                .FirstOrDefaultAsync();

            return invoice;
        }
    }
}
