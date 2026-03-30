using HotelManagement.Context;
using HotelManagement.Models.Common;
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

        public async Task<PagedResult<InvoiceViewModel>> GetAllInvoicesAsync(string? search, int page, int pageSize)
        {
            var query = _context.Invoices
                .Include(i => i.Booking).ThenInclude(b => b.User)
                .Include(i => i.Booking).ThenInclude(b => b.Room)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(i =>
                    (i.Booking.User.FullName != null && i.Booking.User.FullName.ToLower().Contains(s)) ||
                    (i.Booking.User.Phone != null && i.Booking.User.Phone.Contains(s)) ||
                    i.Booking.Room.RoomNumber.ToLower().Contains(s) ||
                    i.Status.ToLower().Contains(s));
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    BookingId = i.BookingId,
                    TotalAmount = i.TotalAmount,
                    PaymentDate = i.PaymentDate,
                    Status = i.Status,
                    CustomerName = i.Booking.User.FullName ?? "(Không tên)",
                    CustomerPhone = i.Booking.User.Phone,
                    Email = i.Booking.User.Email,
                    Address = i.Booking.User.Address,
                    IdCard = i.Booking.User.IDCard,
                    CheckIn = i.Booking.ExpectedCheckIn,
                    CheckOut = i.Booking.ExpectedCheckOut,
                    Deposit = i.Booking.Deposit,
                    RoomNumber = i.Booking.Room.RoomNumber,
                    RoomTypeName = i.Booking.Room.RoomTypeName,
                    RoomPrice = i.Booking.Room.Price,
                    Services = new List<InvoiceServiceItem>()
                })
                .ToListAsync();

            return new PagedResult<InvoiceViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<InvoiceViewModel?> GetInvoiceById(int id)
        {
            var i = await _context.Invoices
                .Include(x => x.Booking).ThenInclude(b => b.User)
                .Include(x => x.Booking).ThenInclude(b => b.Room)
                .Include(x => x.Booking).ThenInclude(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(x => x.InvoiceId == id);

            if (i == null) return null;

            var services = i.Booking.BookingServices
                .Where(bs => bs.Service != null)
                .Select(bs => new InvoiceServiceItem
                {
                    ServiceName = bs.Service!.Name,
                    Price = bs.Service.Price
                })
                .ToList();

            return new InvoiceViewModel
            {
                InvoiceId = i.InvoiceId,
                BookingId = i.BookingId,
                TotalAmount = i.TotalAmount,
                PaymentDate = i.PaymentDate,
                Status = i.Status,
                CustomerName = i.Booking.User?.FullName ?? "(Không tên)",
                CustomerPhone = i.Booking.User?.Phone,
                Email = i.Booking.User?.Email,
                Address = i.Booking.User?.Address,
                IdCard = i.Booking.User?.IDCard,
                CheckIn = i.Booking.ExpectedCheckIn,
                CheckOut = i.Booking.ExpectedCheckOut,
                Deposit = i.Booking.Deposit,
                RoomNumber = i.Booking.Room?.RoomNumber,
                RoomTypeName = i.Booking.Room?.RoomTypeName,
                RoomPrice = i.Booking.Room?.Price,
                Services = services
            };
        }
    }
}
