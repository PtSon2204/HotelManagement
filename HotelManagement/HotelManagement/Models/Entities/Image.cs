namespace HotelManagement.Models.Entities
{
    public class Image
    {
        public int ImageId { get; set; }

        public string Url { get; set; } = null!;

        // Foreign key
        public int RoomId { get; set; }

        // Navigation property
        public Room Room { get; set; } = null!;
    }
}
