namespace HotelManagement.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }

        public int? UserId { get; set; }
        public string? FullName { get; set; }

        public int? RoomId { get; set; }
        public string? RoomNumber { get; set; }

        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? FeedbackDate { get; set; }
    }
}
