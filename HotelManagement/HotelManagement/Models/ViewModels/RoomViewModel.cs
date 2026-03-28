using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.ViewModels
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }
        public int? RoomTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số phòng")]
        [StringLength(50, ErrorMessage = "Số phòng không được vượt quá 50 ký tự")]
        public string RoomNumber { get; set; } = null!;

        public List<string> ImageUrls { get; set; } = new();

        public string? Image => ImageUrls is { Count: > 0 } ? ImageUrls[0] : null;
        public List<RoomImageItem> Images { get; set; } = new List<RoomImageItem>();

        public List<int> DeleteImageIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Vui lòng nhập giá phòng")]
        [Range(0, 999999999, ErrorMessage = "Giá phòng phải lớn hơn 0")]
        public decimal? Price { get; set; }

        public string? Status { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên loại phòng")]
        public string? RoomTypeName { get; set; }

        public int? Capacity { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<RoomTypeItem> RoomTypes { get; set; } = new();
        public List<FeedbackViewModel> Feedbacks { get; set; } = new();
        public double AverageRating => Feedbacks.Count == 0
            ? 0
            : Feedbacks
                .Where(f => f.Rating.HasValue)
                .Select(f => (double)f.Rating!.Value)
                .DefaultIfEmpty(0)
                .Average();
    }

    public sealed class RoomImageItem
    {
        public int ImageId { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public sealed class RoomTypeItem
    {
        public int RoomTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
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
        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? FullName { get; set; }

        public List<RoleLookupItem>? Roles { get; set; }
    }

    public class RoleLookupItem
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
