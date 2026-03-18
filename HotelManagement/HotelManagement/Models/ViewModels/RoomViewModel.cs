namespace HotelManagement.Models.ViewModels
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }

        public int? RoomTypeId { get; set; }

        public string RoomNumber { get; set; } = null!;

        public string? Image { get; set; }

        public decimal? Price { get; set; }

        public string? Status { get; set; }
        public string? RoomTypeName { get; set; }
    }
}
