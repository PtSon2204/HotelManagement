using System.ComponentModel.DataAnnotations;
using HotelManagement.Models.Entities;

namespace HotelManagement.Models.ViewModels
{
    public class DirectBookingViewModel
    {
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
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal? Price { get; set; }

        //thông tin booking
        public int NumberOfPeople { get; set; }
        public DateTime CheckInDate { get; set; } = DateTime.Now;
        public DateTime CheckOutDate { get; set; }
        public int? StaffId { get; set; }

    }
}
