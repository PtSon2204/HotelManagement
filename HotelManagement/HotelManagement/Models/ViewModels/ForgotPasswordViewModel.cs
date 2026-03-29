namespace HotelManagement.Models.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Gmail đã đăng ký")]
        [EmailAddress(ErrorMessage = "Gmail không hợp lệ")]
        public string Email { get; set; } = null!;
    }
}
