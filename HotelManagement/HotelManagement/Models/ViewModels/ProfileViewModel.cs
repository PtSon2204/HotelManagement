using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Ho ten khong duoc de trong.")]
        [Display(Name = "Ho va ten")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Gioi tinh")]
        public string? Gender { get; set; }

        [Display(Name = "Ngay sinh / CMND / CCCD")]
        public string? Idcard { get; set; }

        [Display(Name = "Dia chi")]
        public string? Address { get; set; }

        [Display(Name = "Quoc tich")]
        public string? Nationality { get; set; }

        [Required(ErrorMessage = "Email khong duoc de trong.")]
        [EmailAddress(ErrorMessage = "Email khong hop le.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "So dien thoai")]
        public string? Phone { get; set; }
    }
}
