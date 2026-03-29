using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class DirectBookingViewModel : IValidatableObject
    {
        public int? UserId { get; set; }
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

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }


        public string? AccountName { get; set; }
        public string? AccountPhone { get; set; }

        public int? GuestProfileId { get; set; }

        [Display(Name = "Lưu hồ sơ này thành (VD: Vợ, Bạn, ...)")]
        public string? GuestLabel { get; set; }       

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0901234567)")]
        public string Phone { get; set; } = null!;
        // Tùy chọn lưu thông tin vào profile
        public bool SaveProfile { get; set; } = false;
        public int RoomId { get; set; }

        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng khách")]
        [Range(1, 100, ErrorMessage = "Số lượng khách phải từ 1 đến 100 người")]
        [Display(Name = "Số lượng khách")]
        public int NumberOfPeople { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        public DateTime CheckInDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        public DateTime CheckOutDate { get; set; } = DateTime.Now.AddDays(1);

        public int? StaffId { get; set; }

        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOutDate.Date <= CheckInDate.Date)
                yield return new ValidationResult(
                    "Ngày trả phòng phải sau ngày nhận phòng.",
                    new[] { nameof(CheckOutDate) });
        }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentOption { get; set; } = "Deposit";
    }
}
