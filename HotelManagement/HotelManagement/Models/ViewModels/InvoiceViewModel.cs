using HotelManagement.Models.Entities;

namespace HotelManagement.Models.ViewModels
{
    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfDays
        {
            get
            {
                int days = (CheckOut.Date - CheckIn.Date).Days;
                return days > 0 ? days : 1;
            }
        }
        

        // Thông tin Khách hàng
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? Email {  get; set; }
        public string? Address { get; set; }
        public string? IdCard { get; set; } 

        // Thông tin Đặt phòng 
        public int? BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal? Deposit { get; set; }

        // Thông tin Phòng 
        public string? RoomNumber { get; set; }
        public string? RoomTypeName { get; set; }
        public decimal? RoomPrice { get; set; }

        //Thông tin nhân viên
        public string? StaffName { get; set; }
        public List<InvoiceServiceItem> Services { get; set; } = new List<InvoiceServiceItem>();

        //tính tổng tiền phòng và tiền dịch vụ
        public decimal RoomTotal => (RoomPrice ?? 0) * NumberOfDays;
        public decimal ServiceTotal => Services?.Sum(x => x.Price ?? 0) ?? 0;
    }
    public class InvoiceServiceItem
    {
        public string? ServiceName { get; set; }
        public decimal? Price { get; set; }
    }
}

