using HotelManagement.Models.Entities;

namespace HotelManagement.Models.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        public int? UserId { get; set; }
        public DateTime ExpectedCheckIn { get; set; }
        public DateTime ExpectedCheckOut { get; set; }

        public decimal? Deposit { get; set; }
        public int NumOfPeople { get; set; }
        public string? Status { get; set; }
        public int? StaffId { get; set; }
        public DateTime? CreatedDate { get; set; }

        public Room? Room { get; set; }
        public User? Customer { get; set; }
        public List<Service>? Services { get; set; }
    }
}
