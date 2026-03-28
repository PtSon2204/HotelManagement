using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class DirectBookingViewModel
    {
        public int? UserId { get; set; }

        // ──────────────────────────── Thông tin khách hàng ────────────────────────────
        // --- ACCOUNT INFO (Fixed/Read-only) ---
        public string? AccountName { get; set; }
        public string? AccountPhone { get; set; }

        // --- GUEST INFO (The Actual Person Staying) ---
        public int? GuestProfileId { get; set; }

        [Display(Name = "Lưu hồ sơ này thành (VD: Vợ, Bạn, ...)")]
        public string? GuestLabel { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên khách")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0901234567)")]
        public string Phone { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "CMND / CCCD")]
        [StringLength(20, ErrorMessage = "Số CMND/CCCD không hợp lệ")]
        public string? IdCard { get; set; }

        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? Address { get; set; }

        // Tùy chọn lưu thông tin vào profile
        public bool SaveProfile { get; set; } = false;

        // ──────────────────────────── Thông tin phòng ────────────────────────────
        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng hợp lệ")]
        public int RoomId { get; set; }

        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal? Price { get; set; }

        // ──────────────────────────── Thông tin đặt phòng ────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập số lượng khách")]
        [Range(1, 100, ErrorMessage = "Số lượng khách phải từ 1 đến 100 người")]
        public int NumberOfPeople { get; set; } = 1;

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        public DateTime CheckInDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        public DateTime CheckOutDate { get; set; } = DateTime.Now.AddDays(1);

        public int? StaffId { get; set; }

        // ──────────────────────────── Dịch vụ ────────────────────────────
        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        // ──────────────────────────── Thanh toán ────────────────────────────
        /// <summary>"Deposit" = cọc 50% | "Full" = trả 100% giảm 7%</summary>
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentOption { get; set; } = "Deposit";
    }
}
