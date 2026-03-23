using System.ComponentModel.DataAnnotations;
using HotelManagement.Models.Entities;

namespace HotelManagement.Models.ViewModels
{
    public class DirectBookingViewModel
    {
        public int? CustomerId { get; set; }
        
        //thông tin khách hàng
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = null!;
        public string? IdCard { get; set; }
        public string? Nationality { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }

        //thông tin room
        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng hợp lệ")]
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal? Price { get; set; }

        //thông tin booking
        [Required(ErrorMessage = "Vui lòng nhập số lượng khách")]
        [Range(1, 20, ErrorMessage = "Số lượng khách phải từ 1 đến 20 người")]
        public int NumberOfPeople { get; set; }
        public DateTime CheckInDate { get; set; } = DateTime.Now;
        public DateTime CheckOutDate { get; set; } = DateTime.Now.AddDays(1);
        public int? StaffId { get; set; }

        //thông tin service
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
    }
}
