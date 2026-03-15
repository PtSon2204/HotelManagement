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

    public class ServiceViewModel
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ")]
        [Range(0, 999999999, ErrorMessage = "Giá dịch vụ phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        public bool? IsActive { get; set; }
    }

    public class StaffViewModel
    {
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên nhân viên")]
        [StringLength(100, ErrorMessage = "Họ tên nhân viên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = null!;

        [StringLength(10, ErrorMessage = "Giới tính không được vượt quá 10 ký tự")]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0|\+84)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? Phone { get; set; }

        public string? Image { get; set; }
    }

    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string Username { get; set; } = null!;

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [StringLength(20, ErrorMessage = "Vai trò không được vượt quá 20 ký tự")]
        public string Role { get; set; } = null!;

        public int? CustomerId { get; set; }

        public int? StaffId { get; set; }

        public string? StaffName { get; set; }

        public List<StaffLookupItem>? Staffs { get; set; }
    }

    public class StaffLookupItem
    {
        public int StaffId { get; set; }

        public string FullName { get; set; } = null!;
    }
}
