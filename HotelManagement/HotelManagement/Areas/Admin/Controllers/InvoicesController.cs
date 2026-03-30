using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class InvoicesController : Controller
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly ApplicationDbContext _context;

        public InvoicesController(InvoiceRepository invoiceRepository, ApplicationDbContext context)
        {
            _invoiceRepository = invoiceRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 10;

            // Build query directly for date-filtered path
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

            if (fromDate.HasValue)
                query = query.Where(i => i.PaymentDate.HasValue && i.PaymentDate.Value.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(i => i.PaymentDate.HasValue && i.PaymentDate.Value.Date <= toDate.Value.Date);

            int total = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new Models.ViewModels.InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    BookingId = i.BookingId,
                    TotalAmount = i.TotalAmount,
                    PaymentDate = i.PaymentDate,
                    Status = i.Status,
                    CustomerName = i.Booking.User.FullName ?? "(Không tên)",
                    CustomerPhone = i.Booking.User.Phone,
                    Email = i.Booking.User.Email,
                    RoomNumber = i.Booking.Room.RoomNumber,
                    RoomTypeName = i.Booking.Room.RoomTypeName,
                    RoomPrice = i.Booking.Room.Price,
                    CheckIn = i.Booking.ExpectedCheckIn,
                    CheckOut = i.Booking.ExpectedCheckOut,
                    Deposit = i.Booking.Deposit,
                    Services = new List<Models.ViewModels.InvoiceServiceItem>()
                })
                .ToListAsync();

            var result = new Models.Common.PagedResult<Models.ViewModels.InvoiceViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            // Summary for filtered period
            var filteredTotal = await query.SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;
            var filteredPaid  = await query.Where(i => i.Status == "Paid").SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.FilteredTotal = filteredTotal;
            ViewBag.FilteredPaid  = filteredPaid;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceRepository.GetInvoiceById(id);
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return NotFound();

            if (invoice.Status != "Paid")
            {
                invoice.Status = "Paid";
                invoice.PaymentDate = System.DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xác nhận thanh toán hóa đơn!";
            }

            return RedirectToAction(nameof(Details), new { id = invoiceId });
        }
    }
}
