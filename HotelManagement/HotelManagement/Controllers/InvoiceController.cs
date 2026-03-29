using HotelManagement.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // GET: /Invoice/Payment?bookingId=X
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("LoginRegister", "Account");

            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();

            // Chỉ chủ booking mới được xem
            if (booking.User.Username != username)
                return Forbid();

            int days = (booking.ExpectedCheckOut.Date - booking.ExpectedCheckIn.Date).Days;
            if (days <= 0) days = 1;

            decimal roomTotal    = (booking.Room?.Price ?? 0) * days;
            decimal svcTotal     = booking.BookingServices.Sum(bs => bs.Service?.Price ?? 0);
            decimal grandTotal   = booking.Invoice?.TotalAmount ?? (roomTotal + svcTotal);
            decimal depositToPay = booking.Deposit ?? grandTotal * 0.5m;
            bool    isFullPay    = booking.Deposit.HasValue && booking.Deposit == grandTotal;

            ViewBag.RoomTotal    = roomTotal;
            ViewBag.SvcTotal     = svcTotal;
            ViewBag.GrandTotal   = grandTotal;
            ViewBag.DepositToPay = depositToPay;
            ViewBag.IsFullPay    = isFullPay;
            ViewBag.Days         = days;

            return View(booking);
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // POST: /Invoice/ProcessPayment
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int bookingId, string paymentMethod)
        {
            var booking = await _context.Bookings
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();

            // Cập nhật trạng thái invoice
            if (booking.Invoice != null)
            {
                booking.Invoice.Status      = "Đã thanh toán cọc";
                booking.Invoice.PaymentDate = DateTime.Now;
            }

            booking.Status = "Chờ xác nhận";
            await _context.SaveChangesAsync();

            string methodText = paymentMethod switch {
                "VietQR" => "Chuyển khoản VietQR",
                "CreditCard" => "Thẻ tín dụng Quốc tế",
                "PayPal" => "Ví PayPal",
                _ => "Thanh toán trực tuyến"
            };

            TempData["BookingSuccess"] = $"✅ Thanh toán qua {methodText} thành công! Đang chờ duyệt đặt phòng.";
            return RedirectToAction("MyBookings", "Booking");
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // GET: /Invoice/CancelPayment?bookingId=X
        // ────────────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CancelPayment(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null && (booking.Status == "Chờ xác nhận" || booking.Status == "Pending"))
            {
                booking.Status = "Đã hủy";
                await _context.SaveChangesAsync();
                TempData["Error"] = "Đã hủy đặt phòng.";
            }
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}
