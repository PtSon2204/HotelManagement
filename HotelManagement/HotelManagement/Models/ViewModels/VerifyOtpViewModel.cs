using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm đúng 6 chữ số")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
