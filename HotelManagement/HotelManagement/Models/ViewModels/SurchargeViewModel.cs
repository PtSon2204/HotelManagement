using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class SurchargeViewModel
    {
        public int SurchargeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hóa đơn")]
        [Display(Name = "Mã hóa đơn")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự")]
        [Display(Name = "Lý do")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(0, 999999999, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Số tiền")]
        public decimal Amount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Helper for display
        [Display(Name = "Số hóa đơn")]
        public string? InvoiceNumber { get; set; }
    }
}
