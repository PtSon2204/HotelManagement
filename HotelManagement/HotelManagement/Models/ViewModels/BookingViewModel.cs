using HotelManagement.Models.Entities;

namespace HotelManagement.Models.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime CheckIn { get; set; }

        public DateTime CheckOut { get; set; }

        public decimal? Deposit { get; set; }

        public int NumOfPeople { get; set; }

        public string? Status { get; set; }

        public int? StaffId { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
