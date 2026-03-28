using HotelManagement.Models.Entities;

namespace HotelManagement.Models.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        public int? UserId { get; set; }
        public DateTime ExpectedCheckIn { get; set; }
        public DateTime ExpectedCheckOut { get; set; }
        public DateTime? ActualCheckIn { get; set; }
        public DateTime? ActualCheckOut { get; set; }

        public decimal? Deposit { get; set; }
        public int NumOfPeople { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public Room? Room { get; set; }
        public User? Customer { get; set; }
        public List<Service>? Services { get; set; }
    }
}
