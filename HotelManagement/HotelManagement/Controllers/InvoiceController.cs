using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Context;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagement.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();

            int days = (booking.CheckOut.Date - booking.CheckIn.Date).Days;
            if (days <= 0) days = 1;

            decimal roomTotal = (booking.RoomBookings.FirstOrDefault()?.Room?.Price ?? 0) * days;
            decimal servicesTotal = booking.BookingServices.Sum(bs => bs.Service?.Price ?? 0);
            decimal finalTotal = roomTotal + servicesTotal;
            decimal depositToPay = finalTotal * 0.5m; // 50% deposit

            ViewBag.RoomTotal = roomTotal;
            ViewBag.ServicesTotal = servicesTotal;
            ViewBag.FinalTotal = finalTotal;
            ViewBag.DepositToPay = depositToPay;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int bookingId, string paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(b => b.RoomBookings).ThenInclude(rb => rb.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking != null)
            {
                int days = (booking.CheckOut.Date - booking.CheckIn.Date).Days;
                if (days <= 0) days = 1;

                decimal roomTotal = (booking.RoomBookings.FirstOrDefault()?.Room?.Price ?? 0) * days;
                decimal servicesTotal = booking.BookingServices.Sum(bs => bs.Service?.Price ?? 0);
                decimal finalTotal = roomTotal + servicesTotal;
                decimal depositAmount = finalTotal * 0.5m;

                booking.Deposit = depositAmount;
                booking.Status = "PendingDeposit";
                
                await _context.SaveChangesAsync();
                TempData["BookingSuccess"] = "Xác nhận gửi yêu cầu thanh toán thành công. Đang chờ nhân viên duyệt!";
            }
            return RedirectToAction("MyBookings", "Booking");
        }

        [HttpGet]
        public async Task<IActionResult> CancelPayment(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null && booking.Status == "Pending")
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Error"] = "Đã hủy thao tác đặt phòng do chưa thanh toán khoản cọc.";
            }
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}
