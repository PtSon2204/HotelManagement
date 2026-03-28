namespace HotelManagement.Models.ViewModels
{
    /// <summary>Tóm tắt lịch sử lưu trú của một khách hàng.</summary>
    public class CustomerHistoryViewModel
    {
        // ── Thông tin cơ bản ─────────────────────────────────────
        public int    UserId      { get; set; }
        public string FullName    { get; set; } = null!;
        public string? Phone      { get; set; }
        public string? Email      { get; set; }
        public string? IDCard     { get; set; }
        public string? Gender     { get; set; }
        public string? Nationality{ get; set; }
        public string? Address    { get; set; }

        // ── Thống kê tổng hợp ────────────────────────────────────
        /// Tổng số lần đã ở (chỉ tính booking đã CheckedOut)
        public int TotalCompletedStays { get; set; }

        /// Tổng số đêm tích lũy
        public int TotalNights { get; set; }

        /// Tổng tiền đã chi
        public decimal TotalSpent { get; set; }

        /// Loại phòng hay ở nhất
        public string? FavoriteRoomType { get; set; }

        /// Dịch vụ hay dùng nhất (top 3)
        public List<ServiceUsageStat> TopServices { get; set; } = new();

        // ── Chi tiết từng lần lưu trú ────────────────────────────
        public List<StayRecord> StayHistory { get; set; } = new();
    }

    public class StayRecord
    {
        public int     BookingId        { get; set; }
        public string? RoomNumber       { get; set; }
        public string? RoomTypeName     { get; set; }
        public decimal? RoomPrice       { get; set; }
        public int     NumOfPeople      { get; set; }
        public string? Status           { get; set; }

        public DateTime ExpectedCheckIn  { get; set; }
        public DateTime ExpectedCheckOut { get; set; }
        public DateTime? ActualCheckIn   { get; set; }
        public DateTime? ActualCheckOut  { get; set; }

        /// Số đêm thực tế (dựa theo Actual nếu có, fallback Expected)
        public int Nights
        {
            get
            {
                var co = ActualCheckOut ?? ExpectedCheckOut;
                var ci = ActualCheckIn  ?? ExpectedCheckIn;
                int d  = (co.Date - ci.Date).Days;
                return d > 0 ? d : 1;
            }
        }

        public decimal? TotalAmount { get; set; }   // từ Invoice
        public List<string> Services { get; set; } = new();
    }

    public class ServiceUsageStat
    {
        public string ServiceName { get; set; } = null!;
        public int    UsageCount  { get; set; }
    }
}
