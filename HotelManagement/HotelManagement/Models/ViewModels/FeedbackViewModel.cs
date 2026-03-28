namespace HotelManagement.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public int FeedbackId { get; set; }

        public int? UserId { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? FeedbackDate { get; set; }

        public string? FullName { get; set; }
    }
}
