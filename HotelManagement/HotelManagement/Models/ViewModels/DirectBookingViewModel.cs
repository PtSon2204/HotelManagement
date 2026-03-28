using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class DirectBookingViewModel : IValidatableObject
    {
        public int? UserId { get; set; }

        // ── Thông tin khách hàng ──────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Số CMND/CCCD không quá 20 ký tự")]
        [Display(Name = "CMND / CCCD")]
        public string? IdCard { get; set; }

        [StringLength(50)]
        [Display(Name = "Quốc tịch")]
        public string? Nationality { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        // ── Thông tin phòng ───────────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        [Range(1, int.MaxValue, ErrorMessage = "Phòng không hợp lệ")]
        public int RoomId { get; set; }

        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal? Price { get; set; }

        // ── Thông tin booking ─────────────────────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập số lượng khách")]
        [Range(1, 100, ErrorMessage = "Số lượng khách phải từ 1 đến 100 người")]
        [Display(Name = "Số lượng khách")]
        public int NumberOfPeople { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhận phòng")]
        public DateTime CheckInDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả phòng")]
        public DateTime CheckOutDate { get; set; } = DateTime.Now.AddDays(1);

        public int? StaffId { get; set; }

        // ── Dịch vụ ──────────────────────────────────────────────────
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }

        // ── Custom validation ─────────────────────────────────────────
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOutDate.Date <= CheckInDate.Date)
                yield return new ValidationResult(
                    "Ngày trả phòng phải sau ngày nhận phòng.",
                    new[] { nameof(CheckOutDate) });
        }
    }
}
