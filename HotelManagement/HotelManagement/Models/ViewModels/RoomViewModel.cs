using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
        public int? RoomTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số phòng")]
        [StringLength(50, ErrorMessage = "Số phòng không được vượt quá 50 ký tự")]
        public string RoomNumber { get; set; } = null!;

        public string? Image { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá phòng")]
        [Range(0, 999999999, ErrorMessage = "Giá phòng phải lớn hơn 0")]
        public decimal? Price { get; set; }

        public string? Status { get; set; }
        public string? RoomTypeName { get; set; }
        public List<RoomTypeItem>? RoomTypes { get; set; }
    }

    public class RoomTypeItem
    {
        public int RoomTypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
    }

    public class RoomTypeViewModel
    {
        public int RoomTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên loại phòng")]
        [StringLength(100, ErrorMessage = "Tên loại phòng không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        public string? Image { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, 999999999, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập sức chứa")]
        [Range(1, 100, ErrorMessage = "Sức chứa phải từ 1 đến 100 người")]
        public int Capacity { get; set; }

        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
