using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;

public class Booking
{
    [Key]
    public int BookingId { get; set; }

    // --- Liên kết với bảng User mới ---
    [Required]
    public int UserId { get; set; } // Khách hàng đặt phòng (Thay cho CustomerId)

    [ForeignKey("UserId")]
    public virtual User? Customer { get; set; }

    public int? StaffId { get; set; } // Nhân viên xử lý (Trỏ về bảng User, thay cho StaffId cũ)

    [ForeignKey("StaffId")]
    public virtual User? Staff { get; set; }


    // --- Thời gian dự kiến (Lúc khách đặt) ---
    public DateTime ExpectedCheckIn { get; set; } // Đổi tên cho rõ ràng
    public DateTime ExpectedCheckOut { get; set; }

    // --- Thời gian thực tế (Gộp từ bảng Rental sang) ---
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }


    // --- Thông tin chung ---
    public decimal? Deposit { get; set; } // Tiền cọc
    public int NumOfPeople { get; set; }

    [StringLength(50)]
    public string? Status { get; set; } // Ví dụ: "Pending", "Confirmed", "CheckedIn" (Đang ở), "CheckedOut" (Đã trả phòng), "Cancelled"

    public DateTime? CreatedDate { get; set; } = DateTime.Now;


    // --- Navigation Properties ---
    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    public virtual ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();

    // Hóa đơn giờ sẽ nối thẳng vào Booking thay vì Rental
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
