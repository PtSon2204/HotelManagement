namespace HotelManagement.Models.ViewModels
{
    // Dùng để hiển thị thông tin "khách hàng" (User với Role=Customer)
    public class CustomerViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Gender { get; set; }
        public string? IDCard { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
